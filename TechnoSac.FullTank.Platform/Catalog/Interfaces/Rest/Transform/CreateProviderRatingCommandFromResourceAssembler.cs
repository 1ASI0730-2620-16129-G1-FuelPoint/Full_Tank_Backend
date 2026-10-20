using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Transform;

public static class CreateProviderRatingCommandFromResourceAssembler
{
    public static CreateProviderRatingCommand ToCommandFromResource(CreateProviderRatingResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateProviderRatingCommand(resource.CompanyId, resource.ProviderId, resource.Rating);
    }
}
