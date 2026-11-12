using Microsoft.Extensions.Configuration;
using Npgsql;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Deployment;
using Xunit;

namespace TechnoSac.FullTank.Platform.Tests;

public class DeploymentConfigurationTests
{
    private static Dictionary<string, string?> Settings() => new()
    {
        ["Database:Host"] = "postgres.internal",
        ["Database:Name"] = "av2_test",
        ["Database:User"] = "test_user",
        ["Database:Password"] = "test-only;password=with-separators",
        ["TokenSettings:Secret"] = new string('a', 64),
        ["Cors:AllowedOrigins:0"] = "https://full-tank-964e2.web.app"
    };

    private static IConfiguration Config(Dictionary<string, string?> settings) =>
        new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

    [Fact]
    public void SplitDatabaseSettings_PreservePasswordAndRequireTls()
    {
        var settings = Settings();
        var connection = new NpgsqlConnectionStringBuilder(DeploymentConfiguration.GetConnectionString(Config(settings)));
        Assert.Equal(settings["Database:Password"], connection.Password);
        Assert.Equal(SslMode.Require, connection.SslMode);
        Assert.Equal(20, connection.MaxPoolSize);
        Assert.Equal(5432, connection.Port);
        DeploymentConfiguration.Validate(Config(settings), false);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PublishedSimulation_RequiresExplicitAcademicMode(bool academicDemo)
    {
        var settings = Settings();
        settings["PaymentSettings:EnableSimulation"] = "true";
        settings["AcademicDemo:Enabled"] = academicDemo.ToString();
        var exception = Record.Exception(() => DeploymentConfiguration.Validate(Config(settings), false));
        if (academicDemo) Assert.Null(exception);
        else Assert.IsType<InvalidOperationException>(exception);
    }

    [Theory]
    [InlineData("postgres")]
    [InlineData("postgresql")]
    public void RenderDatabaseUrl_DecodesCredentialsAndUsesTls(string scheme)
    {
        var settings = Settings();
        settings["DATABASE_URL"] = $"{scheme}://demo%40user:p%3Ba%3Ass%40word@db.internal:5433/fulltank%5Fav2?sslmode=require";
        var connection = new NpgsqlConnectionStringBuilder(DeploymentConfiguration.GetConnectionString(Config(settings)));
        Assert.Equal("db.internal", connection.Host);
        Assert.Equal(5433, connection.Port);
        Assert.Equal("demo@user", connection.Username);
        Assert.Equal("p;a:ss@word", connection.Password);
        Assert.Equal("fulltank_av2", connection.Database);
        Assert.Equal(SslMode.Require, connection.SslMode);
        Assert.Equal(20, connection.MaxPoolSize);
    }

    [Fact]
    public void ConnectionString_TakesPrecedenceOverUrlAndSplitSettings()
    {
        var settings = Settings();
        settings["DATABASE_URL"] = "postgresql://ignored:ignored@ignored/url_db";
        settings["ConnectionStrings:DefaultConnection"] = "Host=chosen;Database=chosen_db;Username=app;Password=test-only;SSL Mode=Require";
        var connection = new NpgsqlConnectionStringBuilder(DeploymentConfiguration.GetConnectionString(Config(settings)));
        Assert.Equal("chosen", connection.Host);
        Assert.Equal("chosen_db", connection.Database);
    }

    [Theory]
    [InlineData("Disable")]
    [InlineData("Allow")]
    [InlineData("Prefer")]
    public void PublishedDatabase_RejectsPlaintextFallback(string sslMode)
    {
        var settings = Settings();
        settings["Database:SslMode"] = sslMode;
        Assert.Throws<InvalidOperationException>(() => DeploymentConfiguration.Validate(Config(settings), false));
        DeploymentConfiguration.Validate(Config(settings), true);
    }

    [Fact]
    public void RenderUrlWithoutPort_UsesPostgresqlDefault()
    {
        var connection = new NpgsqlConnectionStringBuilder(DeploymentConfiguration.GetConnectionString(
            Config(new() { ["DATABASE_URL"] = "postgresql://app:test@db.internal/fulltank" })));
        Assert.Equal(5432, connection.Port);
        Assert.Equal(SslMode.Require, connection.SslMode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("short")]
    [InlineData("%JWT_SECRET_LONG_UNRESOLVED_PLACEHOLDER%")]
    public void PublishedApi_RejectsMissingOrPlaceholderJwtSecret(string? secret)
    {
        var settings = Settings();
        settings["TokenSettings:Secret"] = secret;
        Assert.Throws<InvalidOperationException>(() => DeploymentConfiguration.Validate(Config(settings), false));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("*")]
    [InlineData("https://example.com/path")]
    [InlineData("https://example.com/")]
    public void PublishedApi_RejectsMissingOrInvalidCorsOrigin(string? origin)
    {
        var settings = Settings();
        settings["Cors:AllowedOrigins:0"] = origin;
        Assert.Throws<InvalidOperationException>(() => DeploymentConfiguration.Validate(Config(settings), false));
    }

    [Fact]
    public void PublishedSeed_RequiresAcademicModeAndPrivatePassword()
    {
        var settings = Settings();
        settings["Database:SeedDemoData"] = "true";
        Assert.Throws<InvalidOperationException>(() => DeploymentConfiguration.Validate(Config(settings), false));
        settings["AcademicDemo:Enabled"] = "true";
        settings["Database:DemoPassword"] = "123456";
        Assert.Throws<InvalidOperationException>(() => DeploymentConfiguration.Validate(Config(settings), false));
        settings["Database:DemoPassword"] = "unit-test-only-password";
        DeploymentConfiguration.Validate(Config(settings), false);
    }

    [Fact]
    public void RenderPort_BindsAllInterfacesWhileLocalExecutionKeepsItsProfile()
    {
        Assert.Null(DeploymentConfiguration.GetListenUrl(Config(new())));
        Assert.Equal("http://0.0.0.0:12345", DeploymentConfiguration.GetListenUrl(
            Config(new() { ["PORT"] = "12345" })));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("65536")]
    [InlineData("not-a-port")]
    public void RenderPort_RejectsInvalidValues(string port) => Assert.Throws<InvalidOperationException>(() =>
        DeploymentConfiguration.GetListenUrl(Config(new() { ["PORT"] = port })));
}
