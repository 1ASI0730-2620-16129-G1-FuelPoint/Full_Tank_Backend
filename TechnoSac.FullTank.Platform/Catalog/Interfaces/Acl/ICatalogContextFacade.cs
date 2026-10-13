namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Acl;

public interface ICatalogContextFacade
{
    Task<decimal?> FetchProductPricePerLiterAsync(int providerId, string fuelType, CancellationToken cancellationToken);
}
