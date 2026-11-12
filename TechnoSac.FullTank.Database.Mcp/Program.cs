using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

var builder = Host.CreateApplicationBuilder(args);

// STDIO reserves stdout for JSON-RPC messages. Send every log line to stderr.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

var databaseSettings = DatabaseSettings.FromEnvironment();
builder.Services.AddSingleton(databaseSettings);
builder.Services.AddSingleton<DatabaseService>();

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = "fulltank-database-mcp",
            Version = "1.0.0"
        };
        options.ServerInstructions =
            "Local MCP for the fulltank_platform MySQL database. Use read tools to inspect schema and rows before a mutation. " +
            "Write tools only accept parameterized values, require an explicit table allow-list, and updates/deletes require " +
            "a non-empty equality filter plus the exact expected row count. Never try to bypass these safeguards.";
    })
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
