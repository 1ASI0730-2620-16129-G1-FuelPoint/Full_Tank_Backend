using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;

public interface IProviderProductRepository : IBaseRepository<ProviderProduct>
{
    Task<IEnumerable<ProviderProduct>> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken);
    Task<ProviderProduct?> FindByProviderIdAndFuelTypeAsync(int providerId, string fuelType, CancellationToken cancellationToken);
}
