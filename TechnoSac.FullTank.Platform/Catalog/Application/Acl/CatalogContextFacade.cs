using TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Acl;

namespace TechnoSac.FullTank.Platform.Catalog.Application.Acl;

public class CatalogContextFacade(IProviderProductRepository productRepository) : ICatalogContextFacade
{
    public async Task<decimal?> FetchProductPricePerLiterAsync(int providerId, string fuelType,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.FindByProviderIdAndFuelTypeAsync(providerId, fuelType, cancellationToken);
        return product?.PricePerLiter;
    }
}
