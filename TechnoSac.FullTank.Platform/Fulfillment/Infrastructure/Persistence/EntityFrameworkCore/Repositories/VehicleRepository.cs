using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Fulfillment.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class VehicleRepository(AppDbContext context) : BaseRepository<Vehicle>(context), IVehicleRepository
{
    public async Task<IEnumerable<Vehicle>> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken)
    {
        return await Context.Set<Vehicle>()
            .Where(vehicle => vehicle.ProviderId == providerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vehicle>> FindAvailableByProviderIdAsync(int providerId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<Vehicle>()
            .Where(vehicle => vehicle.ProviderId == providerId && vehicle.Status == "AVAILABLE")
            .ToListAsync(cancellationToken);
    }
}
