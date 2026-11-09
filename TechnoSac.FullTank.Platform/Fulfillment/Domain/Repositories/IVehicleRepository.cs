using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;

public interface IVehicleRepository : IBaseRepository<Vehicle>
{
    Task<IEnumerable<Vehicle>> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken);
    Task<IEnumerable<Vehicle>> FindAvailableByProviderIdAsync(int providerId, CancellationToken cancellationToken);
}
