using MySql.Data.MySqlClient;

internal sealed record DatabaseSettings(
    string ConnectionString,
    string Database,
    HashSet<string> WriteTables,
    int MaxRows,
    bool AllowDelete,
    bool AllowSensitiveWrites)
{
    private const string DefaultDatabase = "fulltank_platform";

    public static DatabaseSettings FromEnvironment()
    {
        var rawConnectionString = Environment.GetEnvironmentVariable("FULLTANK_MCP_CONNECTION_STRING");
        MySqlConnectionStringBuilder connectionBuilder;

        if (!string.IsNullOrWhiteSpace(rawConnectionString))
        {
            connectionBuilder = new MySqlConnectionStringBuilder(rawConnectionString);
            var configuredSslMode = Environment.GetEnvironmentVariable("FULLTANK_MCP_DB_SSL_MODE");
            if (!string.IsNullOrWhiteSpace(configuredSslMode))
                connectionBuilder.SslMode = ParseSslMode(configuredSslMode);
            else if (!rawConnectionString.Contains("sslmode", StringComparison.OrdinalIgnoreCase) &&
                     !rawConnectionString.Contains("ssl mode", StringComparison.OrdinalIgnoreCase))
                connectionBuilder.SslMode = MySqlSslMode.Disabled;
        }
        else
        {
            var user = Required("FULLTANK_MCP_DB_USER");
            var password = Required("FULLTANK_MCP_DB_PASSWORD");

            connectionBuilder = new MySqlConnectionStringBuilder
            {
                Server = Environment.GetEnvironmentVariable("FULLTANK_MCP_DB_HOST") ?? "127.0.0.1",
                Port = ParseUInt("FULLTANK_MCP_DB_PORT", 3306),
                Database = Environment.GetEnvironmentVariable("FULLTANK_MCP_DB_NAME") ?? DefaultDatabase,
                UserID = user,
                Password = password,
                ConnectionTimeout = 5,
                DefaultCommandTimeout = 15,
                SslMode = ParseSslMode(Environment.GetEnvironmentVariable("FULLTANK_MCP_DB_SSL_MODE")),
                AllowUserVariables = false
            };
        }

        if (string.IsNullOrWhiteSpace(connectionBuilder.Database))
            connectionBuilder.Database = DefaultDatabase;

        connectionBuilder.ConnectionTimeout = Math.Min(connectionBuilder.ConnectionTimeout, 15u);
        connectionBuilder.DefaultCommandTimeout = Math.Min(connectionBuilder.DefaultCommandTimeout, 30u);
        connectionBuilder.AllowUserVariables = false;
        if (connectionBuilder.SslMode == MySqlSslMode.Disabled && IsLoopback(connectionBuilder.Server))
            connectionBuilder.AllowPublicKeyRetrieval = true;

        var writeTables = ParseCsv("FULLTANK_MCP_WRITE_TABLES");
        var maxRows = Math.Clamp(ParseInt("FULLTANK_MCP_MAX_ROWS", 25), 1, 100);

        return new DatabaseSettings(
            connectionBuilder.ConnectionString,
            connectionBuilder.Database,
            writeTables,
            maxRows,
            ParseBool("FULLTANK_MCP_ALLOW_DELETE"),
            ParseBool("FULLTANK_MCP_ALLOW_SENSITIVE_WRITES"));
    }

    public bool CanWriteTable(string table) =>
        WriteTables.Contains("*") || WriteTables.Contains(table);

    private static string Required(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"Missing {name}. Configure the dedicated MySQL MCP account before starting the server.");
        return value;
    }

    private static HashSet<string> ParseCsv(string name) =>
        new((Environment.GetEnvironmentVariable(name) ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            StringComparer.OrdinalIgnoreCase);

    private static int ParseInt(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : fallback;

    private static uint ParseUInt(string name, uint fallback) =>
        uint.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : fallback;

    private static bool ParseBool(string name) =>
        bool.TryParse(Environment.GetEnvironmentVariable(name), out var value) && value;

    private static MySqlSslMode ParseSslMode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return MySqlSslMode.Disabled;
        if (Enum.TryParse<MySqlSslMode>(value, true, out var mode))
            return mode;
        throw new InvalidOperationException(
            $"Invalid FULLTANK_MCP_DB_SSL_MODE '{value}'. Use Disabled, Preferred, Required, VerifyCA, or VerifyFull.");
    }

    private static bool IsLoopback(string host) =>
        string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
}
