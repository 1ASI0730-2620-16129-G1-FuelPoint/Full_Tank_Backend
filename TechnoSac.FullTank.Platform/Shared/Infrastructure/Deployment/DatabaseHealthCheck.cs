using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace TechnoSac.FullTank.Platform.Shared.Infrastructure.Deployment;

/// <summary>Readiness requires a reachable database with every migration applied.</summary>
public sealed class DatabaseHealthCheck(AppDbContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await context.Database.CanConnectAsync(cancellationToken))
                return HealthCheckResult.Unhealthy("PostgreSQL is unavailable.");
            if ((await context.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
                return HealthCheckResult.Unhealthy("Database migrations are pending.");
            return HealthCheckResult.Healthy();
        }
        catch (Exception)
        {
            // Public health responses must never expose connection details or credentials.
            return HealthCheckResult.Unhealthy("Database readiness could not be verified.");
        }
    }
}
