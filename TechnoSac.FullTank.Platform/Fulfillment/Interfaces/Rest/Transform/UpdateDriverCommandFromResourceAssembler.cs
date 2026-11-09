using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Transform;

public static class UpdateDriverCommandFromResourceAssembler
{
    public static UpdateDriverCommand ToCommandFromResource(int id, UpdateDriverResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateDriverCommand(id, resource.Name, resource.LicenseNumber, resource.Phone, resource.Email,
            resource.Status, resource.ProviderId);
    }
}
