using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Deployment;
using Xunit;

namespace TechnoSac.FullTank.Platform.Tests;

public class DeploymentConfigurationTests
{
    private static Dictionary<string, string?> Settings() => new()
    {
        ["Database:Host"] = "mysql.internal",
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
        var connection = new MySqlConnectionStringBuilder(DeploymentConfiguration.GetConnectionString(Config(settings)));
        Assert.Equal(settings["Database:Password"], connection.Password);
        Assert.Equal(MySqlSslMode.Required, connection.SslMode);
        Assert.Equal((uint)20, connection.MaximumPoolSize);
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
