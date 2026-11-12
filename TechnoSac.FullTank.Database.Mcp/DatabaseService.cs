using System.Data;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Npgsql;

internal sealed class DatabaseService(DatabaseSettings settings, ILogger<DatabaseService> logger)
{
    private static readonly HashSet<string> SensitiveColumnNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "password_hash", "passwordhash", "secret", "token",
        "access_token", "refresh_token", "jwt", "api_key", "apikey"
    };

    public async Task<ListTablesResult> ListTablesAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = @schema
              AND TABLE_TYPE = 'BASE TABLE'
              AND TABLE_NAME <> '__EFMigrationsHistory'
            ORDER BY TABLE_NAME
            """;
        command.Parameters.AddWithValue("@schema", "public");

        var tables = new List<TableSummary>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var table = reader.GetString(0);
            tables.Add(new TableSummary(table, settings.CanWriteTable(table)));
        }

        return new ListTablesResult(settings.Database, tables);
    }

    public async Task<DescribeTableResult> DescribeTableAsync(string table, CancellationToken cancellationToken)
    {
        var metadata = await GetTableMetadataAsync(table, cancellationToken);
        var columns = metadata.Columns.Values
            .Select(column => new ColumnDescription(
                column.Name,
                column.DataType,
                column.ColumnType,
                column.Nullable,
                column.PrimaryKey,
                column.AutoIncrement,
                column.Generated,
                column.Sensitive,
                column.Sensitive ? "[redacted]" : column.DefaultValue))
            .ToArray();

        return new DescribeTableResult(
            metadata.Name,
            settings.CanWriteTable(metadata.Name),
            metadata.PrimaryKey,
            columns);
    }

    public async Task<CountRowsResult> CountRowsAsync(
        string table,
        Dictionary<string, JsonElement>? filters,
        CancellationToken cancellationToken)
    {
        var metadata = await GetTableMetadataAsync(table, cancellationToken);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        var where = BuildWhere(command, metadata, filters, "filter");
        command.CommandText = $"SELECT COUNT(*) FROM {Quote(metadata.Name)}{where}";
        var count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        return new CountRowsResult(metadata.Name, count);
    }

    public async Task<SelectRowsResult> SelectRowsAsync(
        string table,
        string[]? columns,
        Dictionary<string, JsonElement>? filters,
        int limit,
        string? orderBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        var metadata = await GetTableMetadataAsync(table, cancellationToken);
        var selectedColumns = ResolveReadableColumns(metadata, columns);
        var safeLimit = Math.Clamp(limit, 1, settings.MaxRows);

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        var where = BuildWhere(command, metadata, filters, "filter");
        var order = string.Empty;

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            var orderColumn = RequireColumn(metadata, orderBy);
            if (orderColumn.Sensitive)
                throw new InvalidOperationException($"Column '{orderColumn.Name}' is sensitive and cannot be used by this MCP.");
            order = $" ORDER BY {Quote(orderColumn.Name)} {(descending ? "DESC" : "ASC")}";
        }

        command.CommandText =
            $"SELECT {string.Join(", ", selectedColumns.Select(column => Quote(column.Name)))} " +
            $"FROM {Quote(metadata.Name)}{where}{order} LIMIT {safeLimit}";

        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < reader.FieldCount; index++)
                row[reader.GetName(index)] = NormalizeOutput(reader.IsDBNull(index) ? null : reader.GetValue(index));
            rows.Add(row);
        }

        return new SelectRowsResult(metadata.Name, rows.Count, rows);
    }

    public async Task<InsertRowResult> InsertRowAsync(
        string table,
        Dictionary<string, JsonElement> values,
        CancellationToken cancellationToken)
    {
        var metadata = await GetTableMetadataAsync(table, cancellationToken);
        EnsureWriteAllowed(metadata.Name);
        var writableValues = ValidateWriteValues(metadata, values, allowPrimaryKey: true);

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        var parameterNames = new List<string>();
        var columnNames = new List<string>();
        var index = 0;
        foreach (var (column, value) in writableValues)
        {
            var parameterName = $"@value{index++}";
            columnNames.Add(Quote(column.Name));
            parameterNames.Add(parameterName);
            command.Parameters.AddWithValue(parameterName, JsonValueConverter.ToDatabaseValue(value, column.DataType));
        }

        command.CommandText =
            $"INSERT INTO {Quote(metadata.Name)} ({string.Join(", ", columnNames)}) " +
            $"VALUES ({string.Join(", ", parameterNames)})";

        long insertedId = 0;
        int affected;
        if (metadata.PrimaryKey.Count == 1 && metadata.Columns[metadata.PrimaryKey[0]].DataType is "integer" or "bigint" or "smallint")
        {
            command.CommandText += $" RETURNING {Quote(metadata.PrimaryKey[0])}";
            insertedId = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
            affected = 1;
        }
        else affected = await command.ExecuteNonQueryAsync(cancellationToken);
        logger.LogWarning("MCP database insert on {Table}: {AffectedRows} row(s)", metadata.Name, affected);
        return new InsertRowResult(metadata.Name, affected, insertedId);
    }

    public Task<MutationResult> UpdateRowsAsync(
        string table,
        Dictionary<string, JsonElement> filters,
        Dictionary<string, JsonElement> values,
        int expectedRows,
        CancellationToken cancellationToken) =>
        MutateRowsAsync("update", table, filters, values, expectedRows, cancellationToken);

    public Task<MutationResult> DeleteRowsAsync(
        string table,
        Dictionary<string, JsonElement> filters,
        int expectedRows,
        CancellationToken cancellationToken) =>
        MutateRowsAsync("delete", table, filters, null, expectedRows, cancellationToken);

    private async Task<MutationResult> MutateRowsAsync(
        string operation,
        string table,
        Dictionary<string, JsonElement> filters,
        Dictionary<string, JsonElement>? values,
        int expectedRows,
        CancellationToken cancellationToken)
    {
        if (filters.Count == 0)
            throw new InvalidOperationException("Updates and deletes require at least one equality filter.");
        if (expectedRows < 1 || expectedRows > settings.MaxRows)
            throw new InvalidOperationException($"expectedRows must be between 1 and {settings.MaxRows}.");
        if (operation == "delete" && !settings.AllowDelete)
            throw new InvalidOperationException("Deletes are disabled. Set FULLTANK_MCP_ALLOW_DELETE=true to enable them.");

        var metadata = await GetTableMetadataAsync(table, cancellationToken);
        EnsureWriteAllowed(metadata.Name);
        if (metadata.PrimaryKey.Count == 0)
            throw new InvalidOperationException($"Table '{metadata.Name}' has no primary key and cannot be mutated safely.");

        IReadOnlyList<KeyValuePair<ColumnMetadata, JsonElement>> writableValues = operation == "update"
            ? ValidateWriteValues(metadata, values ?? [], allowPrimaryKey: false)
            : [];

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            var keys = await LockTargetKeysAsync(
                connection, transaction, metadata, filters, cancellationToken);

            if (keys.Count != expectedRows)
                throw new InvalidOperationException(
                    $"Mutation cancelled: expected {expectedRows} matching row(s), but found {keys.Count}.");

            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            var keyPredicate = BuildPrimaryKeyPredicate(command, metadata, keys);

            if (operation == "update")
            {
                var assignments = new List<string>();
                var index = 0;
                foreach (var (column, value) in writableValues)
                {
                    var parameterName = $"@value{index++}";
                    assignments.Add($"{Quote(column.Name)} = {parameterName}");
                    command.Parameters.AddWithValue(parameterName, JsonValueConverter.ToDatabaseValue(value, column.DataType));
                }

                command.CommandText =
                    $"UPDATE {Quote(metadata.Name)} SET {string.Join(", ", assignments)} WHERE {keyPredicate}";
            }
            else
            {
                command.CommandText = $"DELETE FROM {Quote(metadata.Name)} WHERE {keyPredicate}";
            }

            var affected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (affected != expectedRows)
                throw new InvalidOperationException(
                    $"Mutation affected {affected} row(s), not the expected {expectedRows}; rolling back.");

            await transaction.CommitAsync(cancellationToken);
            logger.LogWarning(
                "MCP database {Operation} on {Table}: {AffectedRows} row(s)",
                operation,
                metadata.Name,
                affected);
            return new MutationResult(operation, metadata.Name, affected, expectedRows);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task<List<object?[]>> LockTargetKeysAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        TableMetadata metadata,
        Dictionary<string, JsonElement> filters,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        var where = BuildWhere(command, metadata, filters, "filter");
        command.CommandText =
            $"SELECT {string.Join(", ", metadata.PrimaryKey.Select(Quote))} " +
            $"FROM {Quote(metadata.Name)}{where} LIMIT {settings.MaxRows + 1} FOR UPDATE";

        var keys = new List<object?[]>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var key = new object?[metadata.PrimaryKey.Count];
            for (var index = 0; index < key.Length; index++)
                key[index] = reader.IsDBNull(index) ? null : reader.GetValue(index);
            keys.Add(key);
        }

        if (keys.Count > settings.MaxRows)
            throw new InvalidOperationException(
                $"Mutation cancelled: more than {settings.MaxRows} rows match the filter.");

        return keys;
    }

    private static string BuildPrimaryKeyPredicate(
        NpgsqlCommand command,
        TableMetadata metadata,
        IReadOnlyList<object?[]> keys)
    {
        var rows = new List<string>();
        for (var rowIndex = 0; rowIndex < keys.Count; rowIndex++)
        {
            var columns = new List<string>();
            for (var columnIndex = 0; columnIndex < metadata.PrimaryKey.Count; columnIndex++)
            {
                var parameterName = $"@key_{rowIndex}_{columnIndex}";
                columns.Add($"{Quote(metadata.PrimaryKey[columnIndex])} = {parameterName}");
                command.Parameters.AddWithValue(parameterName, keys[rowIndex][columnIndex] ?? DBNull.Value);
            }
            rows.Add($"({string.Join(" AND ", columns)})");
        }

        return string.Join(" OR ", rows);
    }

    private IReadOnlyList<KeyValuePair<ColumnMetadata, JsonElement>> ValidateWriteValues(
        TableMetadata metadata,
        Dictionary<string, JsonElement> values,
        bool allowPrimaryKey)
    {
        if (values.Count == 0)
            throw new InvalidOperationException("At least one value is required.");

        var result = new List<KeyValuePair<ColumnMetadata, JsonElement>>();
        foreach (var (name, value) in values)
        {
            var column = RequireColumn(metadata, name);
            if (column.AutoIncrement || column.Generated)
                throw new InvalidOperationException($"Column '{column.Name}' is generated and cannot be written.");
            if (column.PrimaryKey && !allowPrimaryKey)
                throw new InvalidOperationException($"Primary key column '{column.Name}' cannot be updated.");
            if (column.Sensitive && !settings.AllowSensitiveWrites)
                throw new InvalidOperationException(
                    $"Column '{column.Name}' is sensitive. Set FULLTANK_MCP_ALLOW_SENSITIVE_WRITES=true only if necessary.");
            result.Add(new KeyValuePair<ColumnMetadata, JsonElement>(column, value));
        }
        return result;
    }

    private IReadOnlyList<ColumnMetadata> ResolveReadableColumns(TableMetadata metadata, string[]? columns)
    {
        if (columns is null || columns.Length == 0)
            return metadata.Columns.Values.Where(column => !column.Sensitive).ToArray();

        var result = new List<ColumnMetadata>();
        foreach (var name in columns.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var column = RequireColumn(metadata, name);
            if (column.Sensitive)
                throw new InvalidOperationException($"Column '{column.Name}' is sensitive and cannot be read by this MCP.");
            result.Add(column);
        }
        return result;
    }

    private static string BuildWhere(
        NpgsqlCommand command,
        TableMetadata metadata,
        Dictionary<string, JsonElement>? filters,
        string parameterPrefix)
    {
        if (filters is null || filters.Count == 0)
            return string.Empty;

        var predicates = new List<string>();
        var index = 0;
        foreach (var (name, value) in filters)
        {
            var column = RequireColumn(metadata, name);
            if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                predicates.Add($"{Quote(column.Name)} IS NULL");
                continue;
            }

            var parameterName = $"@{parameterPrefix}{index++}";
            predicates.Add($"{Quote(column.Name)} = {parameterName}");
            command.Parameters.AddWithValue(parameterName, JsonValueConverter.ToDatabaseValue(value, column.DataType));
        }

        return $" WHERE {string.Join(" AND ", predicates)}";
    }

    private async Task<TableMetadata> GetTableMetadataAsync(string table, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("A table name is required.", nameof(table));

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT c.column_name, c.data_type, c.udt_name, c.is_nullable,
                   CASE WHEN EXISTS (
                       SELECT 1 FROM information_schema.table_constraints tc
                       JOIN information_schema.key_column_usage kcu
                         ON tc.constraint_catalog = kcu.constraint_catalog
                        AND tc.constraint_schema = kcu.constraint_schema
                        AND tc.constraint_name = kcu.constraint_name
                       WHERE tc.constraint_type = 'PRIMARY KEY'
                         AND tc.table_schema = c.table_schema AND tc.table_name = c.table_name
                         AND kcu.column_name = c.column_name
                   ) THEN 'PRI' ELSE '' END,
                   CASE WHEN c.is_identity = 'YES' OR c.column_default LIKE 'nextval(%'
                        THEN 'identity' ELSE '' END,
                   c.generation_expression, c.column_default
            FROM information_schema.columns c
            WHERE c.table_schema = @schema AND c.table_name = @table
            ORDER BY c.ordinal_position
            """;
        command.Parameters.AddWithValue("@schema", "public");
        command.Parameters.AddWithValue("@table", table);

        var columns = new Dictionary<string, ColumnMetadata>(StringComparer.OrdinalIgnoreCase);
        var primaryKey = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString(0);
            var key = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
            var extra = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
            var generatedExpression = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
            var isPrimaryKey = string.Equals(key, "PRI", StringComparison.OrdinalIgnoreCase);
            if (isPrimaryKey)
                primaryKey.Add(name);

            columns[name] = new ColumnMetadata(
                name,
                reader.GetString(1),
                reader.GetString(2),
                string.Equals(reader.GetString(3), "YES", StringComparison.OrdinalIgnoreCase),
                isPrimaryKey,
                extra.Equals("identity", StringComparison.OrdinalIgnoreCase),
                !string.IsNullOrWhiteSpace(generatedExpression),
                IsSensitive(name),
                reader.IsDBNull(7) ? null : reader.GetValue(7));
        }

        if (columns.Count == 0 || string.Equals(table, "__EFMigrationsHistory", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Table '{table}' does not exist or is not accessible.");

        return new TableMetadata(table, columns, primaryKey);
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(settings.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private void EnsureWriteAllowed(string table)
    {
        if (!settings.CanWriteTable(table))
            throw new InvalidOperationException(
                $"Writes to '{table}' are disabled. Add it to FULLTANK_MCP_WRITE_TABLES and restart the MCP server.");
    }

    private static ColumnMetadata RequireColumn(TableMetadata metadata, string name)
    {
        if (!metadata.Columns.TryGetValue(name, out var column))
            throw new InvalidOperationException($"Column '{name}' does not exist in table '{metadata.Name}'.");
        return column;
    }

    private static bool IsSensitive(string name) =>
        SensitiveColumnNames.Contains(name) ||
        SensitiveColumnNames.Any(sensitive => name.Contains(sensitive, StringComparison.OrdinalIgnoreCase));

    private static string Quote(string identifier) => "\"" + identifier.Replace("\"", "\"\"") + "\"";

    private static object? NormalizeOutput(object? value) => value switch
    {
        null or DBNull => null,
        byte[] bytes => Convert.ToBase64String(bytes),
        DateTime dateTime => dateTime.ToUniversalTime().ToString("O"),
        DateTimeOffset dateTimeOffset => dateTimeOffset.ToUniversalTime().ToString("O"),
        TimeSpan timeSpan => timeSpan.ToString("c"),
        _ => value
    };
}
