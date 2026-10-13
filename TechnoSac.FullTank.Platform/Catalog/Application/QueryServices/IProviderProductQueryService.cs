using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Catalog.Application.QueryServices;

public interface IProviderProductQueryService
{
    Task<IEnumerable<ProviderProduct>> Handle(GetAllProviderProductsQuery query, CancellationToken cancellationToken);
    Task<ProviderProduct?> Handle(GetProviderProductByIdQuery query, CancellationToken cancellationToken);
}
