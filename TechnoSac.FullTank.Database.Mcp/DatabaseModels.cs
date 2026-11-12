using System.Text.Json;

internal sealed record TableSummary(string Name, bool WriteEnabled);
internal sealed record ListTablesResult(string Database, IReadOnlyList<TableSummary> Tables);

internal sealed record ColumnDescription(
    string Name,
    string DataType,
    string ColumnType,
    bool Nullable,
    bool PrimaryKey,
    bool AutoIncrement,
    bool Generated,
    bool Sensitive,
    object? DefaultValue);

internal sealed record DescribeTableResult(
    string Table,
    bool WriteEnabled,
    IReadOnlyList<string> PrimaryKey,
    IReadOnlyList<ColumnDescription> Columns);

internal sealed record CountRowsResult(string Table, long Count);

internal sealed record SelectRowsResult(
    string Table,
    int ReturnedRows,
    IReadOnlyList<Dictionary<string, object?>> Rows);

internal sealed record InsertRowResult(string Table, int AffectedRows, long LastInsertedId);

internal sealed record MutationResult(
    string Operation,
    string Table,
    int AffectedRows,
    int ExpectedRows);

internal sealed record TableMetadata(
    string Name,
    IReadOnlyDictionary<string, ColumnMetadata> Columns,
    IReadOnlyList<string> PrimaryKey);

internal sealed record ColumnMetadata(
    string Name,
    string DataType,
    string ColumnType,
    bool Nullable,
    bool PrimaryKey,
    bool AutoIncrement,
    bool Generated,
    bool Sensitive,
    object? DefaultValue);

internal static class JsonValueConverter
{
    public static object ToDatabaseValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => DBNull.Value,
        JsonValueKind.String when value.TryGetDateTime(out var dateTime) => dateTime,
        JsonValueKind.String => value.GetString()!,
        JsonValueKind.Number when value.TryGetInt64(out var integer) => integer,
        JsonValueKind.Number when value.TryGetDecimal(out var number) => number,
        JsonValueKind.Number => value.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Array or JsonValueKind.Object => value.GetRawText(),
        _ => value.GetRawText()
    };
}
