using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

[McpServerToolType]
internal static class DatabaseTools
{
    [McpServerTool(
        Name = "db_list_tables",
        Title = "List FullTank database tables",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Lists accessible application tables in fulltank_platform and reports whether each table is enabled for writes.")]
    public static Task<ListTablesResult> ListTables(
        DatabaseService database,
        CancellationToken cancellationToken) =>
        database.ListTablesAsync(cancellationToken);

    [McpServerTool(
        Name = "db_describe_table",
        Title = "Describe a FullTank table",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Returns columns, types, nullability and primary-key metadata for one accessible table. Sensitive defaults are redacted.")]
    public static Task<DescribeTableResult> DescribeTable(
        DatabaseService database,
        [Description("Exact table name returned by db_list_tables.")] string table,
        CancellationToken cancellationToken) =>
        database.DescribeTableAsync(table, cancellationToken);

    [McpServerTool(
        Name = "db_count_rows",
        Title = "Count FullTank rows",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Counts rows using optional equality filters. Call this before update or delete to determine expectedRows.")]
    public static Task<CountRowsResult> CountRows(
        DatabaseService database,
        [Description("Exact table name returned by db_list_tables.")] string table,
        [Description("Optional equality filters as column/value pairs. Null means IS NULL.")] Dictionary<string, JsonElement>? filters,
        CancellationToken cancellationToken) =>
        database.CountRowsAsync(table, filters, cancellationToken);

    [McpServerTool(
        Name = "db_select_rows",
        Title = "Read FullTank rows",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Reads a limited set of rows with equality filters. It does not expose password, secret, token or API-key columns.")]
    public static Task<SelectRowsResult> SelectRows(
        DatabaseService database,
        [Description("Exact table name returned by db_list_tables.")] string table,
        [Description("Columns to return. Omit for all non-sensitive columns.")] string[]? columns,
        [Description("Optional equality filters as column/value pairs. Null means IS NULL.")] Dictionary<string, JsonElement>? filters,
        [Description("Maximum rows to return; the configured server maximum is always enforced.")] int limit = 25,
        [Description("Optional validated column name used for ordering.")] string? orderBy = null,
        [Description("Set true for descending order.")] bool descending = false,
        CancellationToken cancellationToken = default) =>
        database.SelectRowsAsync(table, columns, filters, limit, orderBy, descending, cancellationToken);

    [McpServerTool(
        Name = "db_insert_row",
        Title = "Insert a FullTank row",
        ReadOnly = false,
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Inserts one row into an explicitly write-enabled table using parameterized values. Generated columns are rejected.")]
    public static Task<InsertRowResult> InsertRow(
        DatabaseService database,
        [Description("Exact write-enabled table name returned by db_list_tables.")] string table,
        [Description("Column/value pairs for the new row.")] Dictionary<string, JsonElement> values,
        CancellationToken cancellationToken) =>
        database.InsertRowAsync(table, values, cancellationToken);

    [McpServerTool(
        Name = "db_update_rows",
        Title = "Update FullTank rows",
        ReadOnly = false,
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Updates rows atomically in an explicitly write-enabled table. Requires equality filters and an exact expected row count.")]
    public static Task<MutationResult> UpdateRows(
        DatabaseService database,
        [Description("Exact write-enabled table name returned by db_list_tables.")] string table,
        [Description("Required equality filters as column/value pairs. Null means IS NULL.")] Dictionary<string, JsonElement> filters,
        [Description("Column/value pairs to update. Primary keys and generated columns are rejected.")] Dictionary<string, JsonElement> values,
        [Description("Exact number of rows that must match. Use db_count_rows immediately before this call.")] int expectedRows,
        CancellationToken cancellationToken) =>
        database.UpdateRowsAsync(table, filters, values, expectedRows, cancellationToken);

    [McpServerTool(
        Name = "db_delete_rows",
        Title = "Delete FullTank rows",
        ReadOnly = false,
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Deletes rows atomically when deletes are enabled. Requires equality filters and an exact expected row count.")]
    public static Task<MutationResult> DeleteRows(
        DatabaseService database,
        [Description("Exact write-enabled table name returned by db_list_tables.")] string table,
        [Description("Required equality filters as column/value pairs. Null means IS NULL.")] Dictionary<string, JsonElement> filters,
        [Description("Exact number of rows that must match. Use db_count_rows immediately before this call.")] int expectedRows,
        CancellationToken cancellationToken) =>
        database.DeleteRowsAsync(table, filters, expectedRows, cancellationToken);
}
