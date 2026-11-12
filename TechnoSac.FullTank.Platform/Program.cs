using System.Text.Json;
using TechnoSac.FullTank.Platform.Catalog.Application.CommandServices;
using TechnoSac.FullTank.Platform.Catalog.Application.Internal.CommandServices;
using TechnoSac.FullTank.Platform.Catalog.Application.Internal.QueryServices;
using TechnoSac.FullTank.Platform.Catalog.Application.QueryServices;
using TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;
using TechnoSac.FullTank.Platform.Catalog.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using TechnoSac.FullTank.Platform.Equipment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Equipment.Application.Internal.CommandServices;
using TechnoSac.FullTank.Platform.Equipment.Application.Internal.QueryServices;
using TechnoSac.FullTank.Platform.Equipment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Equipment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Equipment.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using TechnoSac.FullTank.Platform.Fulfillment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Fulfillment.Application.Internal.CommandServices;
using TechnoSac.FullTank.Platform.Fulfillment.Application.Internal.QueryServices;
using TechnoSac.FullTank.Platform.Fulfillment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Fulfillment.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using TechnoSac.FullTank.Platform.Catalog.Application.Acl;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Equipment.Application.Acl;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Fulfillment.Application.Acl;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Inventory.Application.Acl;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Notification.Application.Acl;
using TechnoSac.FullTank.Platform.Notification.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Iam.Application.Acl;
using TechnoSac.FullTank.Platform.Iam.Application.CommandServices;
using TechnoSac.FullTank.Platform.Iam.Application.Internal.CommandServices;
using TechnoSac.FullTank.Platform.Iam.Application.Internal.OutboundServices;
using TechnoSac.FullTank.Platform.Iam.Application.Internal.QueryServices;
using TechnoSac.FullTank.Platform.Iam.Application.QueryServices;
using TechnoSac.FullTank.Platform.Iam.Domain.Repositories;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Hashing.BCrypt.Services;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Tokens.Jwt.Configuration;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Tokens.Jwt.Services;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Tokens.Registration;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Iam.Resources;
using TechnoSac.FullTank.Platform.Notification.Application.CommandServices;
using TechnoSac.FullTank.Platform.Notification.Application.Internal.CommandServices;
using TechnoSac.FullTank.Platform.Notification.Application.Internal.QueryServices;
using TechnoSac.FullTank.Platform.Notification.Application.QueryServices;
using TechnoSac.FullTank.Platform.Notification.Domain.Repositories;
using TechnoSac.FullTank.Platform.Notification.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using TechnoSac.FullTank.Platform.Inventory.Application.CommandServices;
using TechnoSac.FullTank.Platform.Inventory.Application.Internal.CommandServices;
using TechnoSac.FullTank.Platform.Inventory.Application.Internal.QueryServices;
using TechnoSac.FullTank.Platform.Inventory.Application.QueryServices;
using TechnoSac.FullTank.Platform.Inventory.Domain.Repositories;
using TechnoSac.FullTank.Platform.Inventory.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using TechnoSac.FullTank.Platform.Ordering.Application.CommandServices;
using TechnoSac.FullTank.Platform.Ordering.Application.Acl;
using TechnoSac.FullTank.Platform.Ordering.Application.Internal.CommandServices;
using TechnoSac.FullTank.Platform.Ordering.Application.Internal.QueryServices;
using TechnoSac.FullTank.Platform.Ordering.Application.QueryServices;
using TechnoSac.FullTank.Platform.Ordering.Domain.Repositories;
using TechnoSac.FullTank.Platform.Ordering.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Payment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Payment.Application.Internal.CommandServices;
using TechnoSac.FullTank.Platform.Payment.Application.Internal.QueryServices;
using TechnoSac.FullTank.Platform.Payment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Payment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Payment.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Application.Internal.QueryServices;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Application.OutboundServices;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Application.QueryServices;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Infrastructure.Persistence.EntityFrameworkCore;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Resources.Shared;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;
using TechnoSac.FullTank.Platform.Ordering.Application.Internal.EventHandlers;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Events;
using TechnoSac.FullTank.Platform.Shared.Application.Internal.EventBus;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Pipeline.Middleware.Extensions;
using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Deployment;
using ProblemDetailsFactory = TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;

var builder = WebApplication.CreateBuilder(args);

// Load developer-specific local configuration strictly in Development environment.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}
// Re-add environment variables and command-line arguments so deployment/runtime overrides win.
builder.Configuration.AddEnvironmentVariables();
if (args is { Length: > 0 })
{
    builder.Configuration.AddCommandLine(args);
}

DeploymentConfiguration.Validate(builder.Configuration, builder.Environment.IsDevelopment());
var listenUrl = DeploymentConfiguration.GetListenUrl(builder.Configuration);
if (listenUrl is not null) builder.WebHost.UseUrls(listenUrl);
var academicDemo = builder.Configuration.GetValue<bool>("AcademicDemo:Enabled");

const string allowFrontendPolicy = "AllowFrontendPolicy";

// Add services to the container.
builder.Services.AddDataProtection();

// Routing: lowercase URLs + kebab-case controller route naming convention.
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddJsonOptions(options =>
    {
        // JSON responses in camelCase for Vue frontend compatibility.
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    })
    .AddDataAnnotationsLocalization();

// Add ProblemDetails services (RFC 7807).
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("postgresql", timeout: TimeSpan.FromSeconds(5));

// Add CORS Policy for the frontend, reading allowed origins from configuration.
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
var allowedOrigins = configuredOrigins ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(allowFrontendPolicy,
        policy => policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Add Database Connection.
// Configure Database Context and route EF logs through the app logger pipeline.
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionString = DeploymentConfiguration.GetConnectionString(builder.Configuration);
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Database connection string is not set in the configuration.");

    options.UseNpgsql(connectionString)
        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
        .EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

// Localization configuration.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Explicitly register IStringLocalizer for ErrorMessages and CommonMessages.
builder.Services.AddSingleton<IStringLocalizer<ErrorMessages>, StringLocalizer<ErrorMessages>>();
builder.Services.AddSingleton<IStringLocalizer<CommonMessages>, StringLocalizer<CommonMessages>>();
builder.Services.AddSingleton<IStringLocalizer<IamMessages>, StringLocalizer<IamMessages>>();

// Register the custom localized ProblemDetailsFactory.
builder.Services.AddSingleton<ProblemDetailsFactory>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "FullTank API",
            Version = "v1",
            Description = academicDemo
                ? "FullTank AV2 academic demo. Payments, dispatch tracking and receipts are simulated; no money is processed and receipts are not SUNAT electronic invoices."
                : "FullTank Platform API by FuelPoint",
            Contact = new OpenApiContact
            {
                Name = "FuelPoint"
            },
            License = new OpenApiLicense
            {
                Name = "Apache 2.0",
                Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
            }
        });
    // JWT Bearer security scheme prepared for the IAM bounded context (next phase).
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] });
    options.EnableAnnotations();
});

// Dependency Injection.

// Shared Bounded Context.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEventBus, EventBus>();

// Iam Bounded Context.
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IBuyerCompanyRepository, BuyerCompanyRepository>();
builder.Services.AddScoped<IBuyerCompanyCommandService, BuyerCompanyCommandService>();
builder.Services.AddScoped<IBuyerCompanyQueryService, BuyerCompanyQueryService>();
builder.Services.AddScoped<IProviderCompanyRepository, ProviderCompanyRepository>();
builder.Services.AddScoped<IProviderCompanyCommandService, ProviderCompanyCommandService>();
builder.Services.AddScoped<IProviderCompanyQueryService, ProviderCompanyQueryService>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRegistrationTokenService, RegistrationTokenService>();
builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();

// Catalog Bounded Context.
builder.Services.AddScoped<IProviderProductRepository, ProviderProductRepository>();
builder.Services.AddScoped<IProviderProductCommandService, ProviderProductCommandService>();
builder.Services.AddScoped<IProviderProductQueryService, ProviderProductQueryService>();
builder.Services.AddScoped<IFavoriteProviderRepository, FavoriteProviderRepository>();
builder.Services.AddScoped<IFavoriteProviderCommandService, FavoriteProviderCommandService>();
builder.Services.AddScoped<IFavoriteProviderQueryService, FavoriteProviderQueryService>();
builder.Services.AddScoped<IProviderRatingRepository, ProviderRatingRepository>();
builder.Services.AddScoped<IProviderRatingCommandService, ProviderRatingCommandService>();
builder.Services.AddScoped<IProviderRatingQueryService, ProviderRatingQueryService>();
builder.Services.AddScoped<ICatalogContextFacade, CatalogContextFacade>();

// Inventory Bounded Context.
builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
builder.Services.AddScoped<IInventoryItemCommandService, InventoryItemCommandService>();
builder.Services.AddScoped<IInventoryItemQueryService, InventoryItemQueryService>();
builder.Services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
builder.Services.AddScoped<IInventoryMovementCommandService, InventoryMovementCommandService>();
builder.Services.AddScoped<IInventoryMovementQueryService, InventoryMovementQueryService>();
builder.Services.AddScoped<IInventoryContextFacade, InventoryContextFacade>();

// Equipment Bounded Context.
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<IEquipmentCommandService, EquipmentCommandService>();
builder.Services.AddScoped<IEquipmentQueryService, EquipmentQueryService>();
builder.Services.AddScoped<IRefillHistoryRepository, RefillHistoryRepository>();
builder.Services.AddScoped<IRefillHistoryCommandService, RefillHistoryCommandService>();
builder.Services.AddScoped<IRefillHistoryQueryService, RefillHistoryQueryService>();
builder.Services.AddScoped<IEquipmentContextFacade, EquipmentContextFacade>();

// Ordering Bounded Context.
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderCommandService, OrderCommandService>();
builder.Services.AddScoped<IOrderQueryService, OrderQueryService>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IRequestCommandService, RequestCommandService>();
builder.Services.AddScoped<IRequestQueryService, RequestQueryService>();
builder.Services.AddScoped<IOrderingContextFacade, OrderingContextFacade>();
builder.Services.AddScoped<OrderWorkflowEventHandler>();
builder.Services.AddScoped<Cortex.Mediator.Notifications.INotificationHandler<RequestApprovedEvent>>(sp => sp.GetRequiredService<OrderWorkflowEventHandler>());
builder.Services.AddScoped<Cortex.Mediator.Notifications.INotificationHandler<OrderDispatchedEvent>>(sp => sp.GetRequiredService<OrderWorkflowEventHandler>());
builder.Services.AddScoped<Cortex.Mediator.Notifications.INotificationHandler<OrderReceivedEvent>>(sp => sp.GetRequiredService<OrderWorkflowEventHandler>());
builder.Services.AddScoped<Cortex.Mediator.Notifications.INotificationHandler<OrderCancelledEvent>>(sp => sp.GetRequiredService<OrderWorkflowEventHandler>());

// Fulfillment Bounded Context.
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IDriverCommandService, DriverCommandService>();
builder.Services.AddScoped<IDriverQueryService, DriverQueryService>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleCommandService, VehicleCommandService>();
builder.Services.AddScoped<IVehicleQueryService, VehicleQueryService>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IDeliveryCommandService, DeliveryCommandService>();
builder.Services.AddScoped<IDeliveryQueryService, DeliveryQueryService>();
builder.Services.AddScoped<IFulfillmentContextFacade, FulfillmentContextFacade>();

// Payment Bounded Context.
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentCommandService, PaymentCommandService>();
builder.Services.AddScoped<IPaymentCheckoutService, PaymentCheckoutService>();
builder.Services.AddScoped<IPaymentQueryService, PaymentQueryService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceCommandService, InvoiceCommandService>();
builder.Services.AddScoped<IInvoiceQueryService, InvoiceQueryService>();

// Notification Bounded Context.
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationCommandService, NotificationCommandService>();
builder.Services.AddScoped<INotificationQueryService, NotificationQueryService>();
builder.Services.AddScoped<INotificationContextFacade, NotificationContextFacade>();

// ReportingAndAnalytics Bounded Context (read-only; no persisted entities yet).
builder.Services.AddScoped<IAnalyticsReadStore, AnalyticsReadStore>();
builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();

// Mediator Configuration (Cortex).
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
builder.Services.AddCortexMediator(
    [typeof(Program)]);

var app = builder.Build();
if (academicDemo)
    app.Logger.LogWarning("AV2 academic demo enabled. Payments and receipts are simulated; no real money is processed.");

// On Render, a failed migration/seed must fail deployment rather than publish a broken API.
var applyMigrations = builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
var seedDemoData = builder.Configuration.GetValue<bool?>("Database:SeedDemoData")
                   ?? (app.Environment.IsDevelopment() && applyMigrations);
if (applyMigrations || seedDemoData)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (applyMigrations) context.Database.Migrate();
        if (seedDemoData)
        {
            DatabaseSeeder.Seed(context, builder.Configuration["Database:DemoPassword"] ?? "123456");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex,
            "Could not apply database migrations on startup. Ensure PostgreSQL is running and the connection string is correct.");
        if (!app.Environment.IsDevelopment())
            throw new InvalidOperationException("Database initialization failed; deployment cannot become ready.", ex);
    }
}

// Configure the HTTP request pipeline.

// Global exception handler (localized RFC 7807 ProblemDetails) — first in the pipeline.
app.UseGlobalExceptionHandler();

var supportedCultures = new[] { "en-US", "es-419", "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
localizationOptions.FallBackToParentCultures = true;
localizationOptions.FallBackToParentUICultures = true;
app.UseRequestLocalization(localizationOptions);

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply CORS Policy.
app.UseCors(allowFrontendPolicy);

// IAM: best-effort token reader (populates HttpContext.Items["User"]); [Authorize] enforces access.
app.UseRequestAuthorization();

// Render terminates TLS and redirects HTTP at its edge; container traffic is HTTP.
if (builder.Configuration.GetValue("Hosting:HttpsRedirectionEnabled", true))
    app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/health/live", () => Results.Ok(new { status = "alive" }));

app.Run();
