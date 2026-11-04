using MySql.Data.MySqlClient;

namespace TechnoSac.FullTank.Platform.Shared.Infrastructure.Deployment;

/// <summary>Runtime configuration shared by local execution and the AV2 container.</summary>
public static class DeploymentConfiguration
{
    public static string? GetConnectionString(IConfiguration configuration)
    {
        var existing = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(existing))
            return Environment.ExpandEnvironmentVariables(existing);

        var host = configuration["Database:Host"];
        if (string.IsNullOrWhiteSpace(host)) return null;

        var port = configuration.GetValue("Database:Port", 3306);
        if (port is < 1 or > 65535)
            throw new InvalidOperationException("Database:Port must be between 1 and 65535.");

        return new MySqlConnectionStringBuilder
        {
            Server = host,
            Port = (uint)port,
            Database = configuration["Database:Name"],
            UserID = configuration["Database:User"],
            Password = configuration["Database:Password"],
            SslMode = Enum.Parse<MySqlSslMode>(configuration["Database:SslMode"] ?? "Required", true),
            ConnectionTimeout = 10,
            DefaultCommandTimeout = 30,
            MaximumPoolSize = 20
        }.ConnectionString;
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
            throw new InvalidOperationException("Configure a real MySQL connection using ConnectionStrings:DefaultConnection or Database:Host/Name/User/Password.");

        var mysql = new MySqlConnectionStringBuilder(connection);
        if (string.IsNullOrWhiteSpace(mysql.Database) || string.IsNullOrWhiteSpace(mysql.UserID) ||
            string.IsNullOrWhiteSpace(mysql.Password))
            throw new InvalidOperationException("MySQL database name, user and password are required outside Development.");

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
