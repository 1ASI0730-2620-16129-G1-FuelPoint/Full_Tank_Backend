using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Transform;

public static class ProviderProductResourceFromEntityAssembler
{
    public static ProviderProductResource ToResourceFromEntity(ProviderProduct entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new ProviderProductResource(entity.Id, entity.ProviderId, entity.FuelType, entity.Name,
            entity.Description, entity.PricePerLiter, entity.Unit, entity.Available, entity.CreatedAt, entity.UpdatedAt);
    }
}
