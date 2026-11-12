using Npgsql;

namespace TechnoSac.FullTank.Platform.Shared.Infrastructure.Deployment;

/// <summary>Runtime configuration shared by local execution and the AV2 container.</summary>
public static class DeploymentConfiguration
{
    public static string? GetConnectionString(IConfiguration configuration)
    {
        var existing = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(existing))
            return ParseConnectionString(Environment.ExpandEnvironmentVariables(existing));

        var databaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
            return ParseConnectionString(databaseUrl);

        var host = configuration["Database:Host"];
        if (string.IsNullOrWhiteSpace(host)) return null;

        var port = configuration.GetValue("Database:Port", 5432);
        if (port is < 1 or > 65535)
            throw new InvalidOperationException("Database:Port must be between 1 and 65535.");

        return new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = configuration["Database:Name"],
            Username = configuration["Database:User"],
            Password = configuration["Database:Password"],
            SslMode = Enum.Parse<SslMode>(configuration["Database:SslMode"] ?? "Require", true),
            Timeout = 10,
            CommandTimeout = 30,
            MaxPoolSize = 20
        }.ConnectionString;
    }

    private static string ParseConnectionString(string value)
    {
        NpgsqlConnectionStringBuilder connection;
        if (value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                string.IsNullOrWhiteSpace(uri.Host))
                throw new InvalidOperationException("Invalid PostgreSQL database URL.");
            var credentials = uri.UserInfo.Split(':', 2);
            connection = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.IsDefaultPort ? 5432 : uri.Port,
                Username = Uri.UnescapeDataString(credentials[0]),
                Password = credentials.Length == 2 ? Uri.UnescapeDataString(credentials[1]) : null,
                Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
                SslMode = SslMode.Require
            };
            foreach (var part in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = part.Split('=', 2);
                if (pair.Length != 2 || !pair[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Only sslmode is supported in a PostgreSQL URL query. Use an Npgsql connection string for other options.");
                connection.SslMode = Enum.Parse<SslMode>(Uri.UnescapeDataString(pair[1]), true);
            }
        }
        else connection = new NpgsqlConnectionStringBuilder(value);

        connection.MaxPoolSize = Math.Min(connection.MaxPoolSize, 20);
        connection.Timeout = 10;
        connection.CommandTimeout = 30;
        return connection.ConnectionString;
    }

    public static string? GetListenUrl(IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration["PORT"])) return null;
        if (!int.TryParse(configuration["PORT"], out var port) || port is < 1 or > 65535)
            throw new InvalidOperationException("PORT must be between 1 and 65535.");
        return $"http://0.0.0.0:{port}";
    }

    public static void Validate(IConfiguration configuration, bool isDevelopment)
    {
        if (isDevelopment) return;

        var connection = GetConnectionString(configuration);
        if (string.IsNullOrWhiteSpace(connection) || connection.Contains("%DATABASE_"))
            throw new InvalidOperationException("Configure PostgreSQL using ConnectionStrings:DefaultConnection, DATABASE_URL or Database:Host/Name/User/Password.");

        var postgres = new NpgsqlConnectionStringBuilder(connection);
        if (string.IsNullOrWhiteSpace(postgres.Host) || string.IsNullOrWhiteSpace(postgres.Database) ||
            string.IsNullOrWhiteSpace(postgres.Username) || string.IsNullOrWhiteSpace(postgres.Password))
            throw new InvalidOperationException("PostgreSQL host, database name, user and password are required outside Development.");
        if (postgres.SslMode is SslMode.Disable or SslMode.Allow or SslMode.Prefer)
            throw new InvalidOperationException("PostgreSQL connections outside Development must require TLS.");

        var jwtSecret = configuration["TokenSettings:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32 ||
            jwtSecret.Contains('%') || jwtSecret.Contains("secret-key", StringComparison.OrdinalIgnoreCase) ||
            jwtSecret.Contains("replace-me", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("TokenSettings:Secret must be a private random key of at least 32 characters.");

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length == 0 || origins.Any(origin =>
                !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                uri.Scheme != "https" || origin != uri.GetLeftPart(UriPartial.Authority)))
            throw new InvalidOperationException("Cors:AllowedOrigins must contain explicit HTTPS origins without paths or trailing slashes.");

        var academicDemo = configuration.GetValue<bool>("AcademicDemo:Enabled");
        if (configuration.GetValue<bool>("PaymentSettings:EnableSimulation") && !academicDemo)
            throw new InvalidOperationException("Simulated payments outside Development require AcademicDemo:Enabled=true. No real payments are processed.");

        if (configuration.GetValue<bool>("Database:SeedDemoData"))
        {
            if (!academicDemo)
                throw new InvalidOperationException("Demo data outside Development requires AcademicDemo:Enabled=true.");
            if (string.IsNullOrWhiteSpace(configuration["Database:DemoPassword"]) ||
                configuration["Database:DemoPassword"]!.Length < 12)
                throw new InvalidOperationException("Database:DemoPassword must contain at least 12 characters for published demo accounts.");
        }
    }
}
