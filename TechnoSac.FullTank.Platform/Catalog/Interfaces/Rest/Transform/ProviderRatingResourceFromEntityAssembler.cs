using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Transform;

public static class ProviderRatingResourceFromEntityAssembler
{
    public static ProviderRatingResource ToResourceFromEntity(ProviderRating entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new ProviderRatingResource(entity.Id, entity.CompanyId, entity.ProviderId, entity.Rating,
            entity.CreatedAt, entity.UpdatedAt);
    }
}
