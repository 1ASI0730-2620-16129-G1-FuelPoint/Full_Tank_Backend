using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Transform;

public static class CreateFavoriteProviderCommandFromResourceAssembler
{
    public static CreateFavoriteProviderCommand ToCommandFromResource(CreateFavoriteProviderResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateFavoriteProviderCommand(resource.CompanyId, resource.ProviderId);
    }
}
