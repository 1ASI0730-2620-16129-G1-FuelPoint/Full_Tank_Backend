using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Transform;

public static class CreateDriverCommandFromResourceAssembler
{
    public static CreateDriverCommand ToCommandFromResource(CreateDriverResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateDriverCommand(resource.Name, resource.LicenseNumber, resource.Phone, resource.Email,
            resource.Status, resource.ProviderId);
    }
}
