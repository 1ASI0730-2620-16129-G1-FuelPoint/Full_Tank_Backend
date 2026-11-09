using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Transform;

public static class FavoriteProviderResourceFromEntityAssembler
{
    public static FavoriteProviderResource ToResourceFromEntity(FavoriteProvider entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new FavoriteProviderResource(entity.Id, entity.CompanyId, entity.ProviderId, entity.CreatedAt,
            entity.UpdatedAt);
    }
}
