using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;

public interface IDriverRepository : IBaseRepository<Driver>
{
    Task<IEnumerable<Driver>> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken);
}
