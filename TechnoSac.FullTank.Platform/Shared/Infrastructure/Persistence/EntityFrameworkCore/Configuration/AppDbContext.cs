using TechnoSac.FullTank.Platform.Catalog.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using TechnoSac.FullTank.Platform.Equipment.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using TechnoSac.FullTank.Platform.Fulfillment.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using TechnoSac.FullTank.Platform.Inventory.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using TechnoSac.FullTank.Platform.Notification.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using TechnoSac.FullTank.Platform.Ordering.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using TechnoSac.FullTank.Platform.Payment.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Application database context shared by all bounded contexts.
/// </summary>
/// <remarks>
///     Each bounded context contributes its own entity mappings through an <c>ApplyXxxConfiguration</c>
///     extension method, so this context never concentrates all table configuration in one place.
///     The <see cref="AuditableEntityInterceptor" /> populates <c>CreatedAt</c>/<c>UpdatedAt</c> automatically.
/// </remarks>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Apply audit timestamp interceptor for all IAuditableEntity implementations.
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Per-bounded-context model configuration.
        // Each bounded context contributes its own ApplyXxxConfiguration extension method
        // (defined in <Context>/Infrastructure/Persistence/EntityFrameworkCore/Configuration/Extensions).
        // Uncomment each line as the corresponding bounded context is implemented in the next phases:
        builder.ApplyIamConfiguration();
        builder.ApplyCatalogConfiguration();
        builder.ApplyInventoryConfiguration();
        builder.ApplyEquipmentConfiguration();
        builder.ApplyOrderingConfiguration();
        builder.ApplyFulfillmentConfiguration();
        builder.ApplyPaymentConfiguration();
        builder.ApplyNotificationConfiguration();
        // builder.ApplyReportingAndAnalyticsConfiguration();

        // General snake_case naming convention for all database objects.
        builder.UseSnakeCaseNamingConvention();
    }
}
