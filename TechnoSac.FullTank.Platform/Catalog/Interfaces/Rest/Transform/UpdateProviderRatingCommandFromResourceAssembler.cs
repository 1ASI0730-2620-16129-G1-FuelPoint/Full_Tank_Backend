using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Transform;

public static class UpdateProviderRatingCommandFromResourceAssembler
{
    public static UpdateProviderRatingCommand ToCommandFromResource(int id, UpdateProviderRatingResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateProviderRatingCommand(id, resource.CompanyId, resource.ProviderId, resource.Rating);
    }
}
