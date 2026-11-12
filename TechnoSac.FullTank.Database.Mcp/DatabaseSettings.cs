using Npgsql;

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
        NpgsqlConnectionStringBuilder connectionBuilder;

        if (!string.IsNullOrWhiteSpace(rawConnectionString))
        {
            connectionBuilder = new NpgsqlConnectionStringBuilder(rawConnectionString);
            var configuredSslMode = Environment.GetEnvironmentVariable("FULLTANK_MCP_DB_SSL_MODE");
            if (!string.IsNullOrWhiteSpace(configuredSslMode))
                connectionBuilder.SslMode = ParseSslMode(configuredSslMode);
            else if (!rawConnectionString.Contains("sslmode", StringComparison.OrdinalIgnoreCase) &&
                     !rawConnectionString.Contains("ssl mode", StringComparison.OrdinalIgnoreCase))
                connectionBuilder.SslMode = SslMode.Require;
        }
        else
        {
            var user = Required("FULLTANK_MCP_DB_USER");
            var password = Required("FULLTANK_MCP_DB_PASSWORD");

            connectionBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = Environment.GetEnvironmentVariable("FULLTANK_MCP_DB_HOST") ?? "127.0.0.1",
                Port = ParseInt("FULLTANK_MCP_DB_PORT", 5432),
                Database = Environment.GetEnvironmentVariable("FULLTANK_MCP_DB_NAME") ?? DefaultDatabase,
                Username = user,
                Password = password,
                Timeout = 5,
                CommandTimeout = 15,
                SslMode = ParseSslMode(Environment.GetEnvironmentVariable("FULLTANK_MCP_DB_SSL_MODE"))
            };
        }

        if (string.IsNullOrWhiteSpace(connectionBuilder.Database))
            connectionBuilder.Database = DefaultDatabase;

        connectionBuilder.Timeout = Math.Min(connectionBuilder.Timeout, 15);
        connectionBuilder.CommandTimeout = Math.Min(connectionBuilder.CommandTimeout, 30);
        connectionBuilder.MaxPoolSize = Math.Min(connectionBuilder.MaxPoolSize, 5);
        connectionBuilder.SearchPath = "public";

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
                $"Missing {name}. Configure the dedicated PostgreSQL MCP account before starting the server.");
        return value;
    }

    private static HashSet<string> ParseCsv(string name) =>
        new((Environment.GetEnvironmentVariable(name) ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            StringComparer.OrdinalIgnoreCase);

    private static int ParseInt(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : fallback;

    private static bool ParseBool(string name) =>
        bool.TryParse(Environment.GetEnvironmentVariable(name), out var value) && value;

    private static SslMode ParseSslMode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return SslMode.Require;
        if (Enum.TryParse<SslMode>(value, true, out var mode))
            return mode;
        throw new InvalidOperationException(
            $"Invalid FULLTANK_MCP_DB_SSL_MODE '{value}'. Use Disable, Prefer, Require, VerifyCA, or VerifyFull.");
    }

}
