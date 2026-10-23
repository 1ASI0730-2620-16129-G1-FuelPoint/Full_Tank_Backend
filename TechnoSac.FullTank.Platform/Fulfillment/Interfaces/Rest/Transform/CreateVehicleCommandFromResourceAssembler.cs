using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Transform;

public static class CreateVehicleCommandFromResourceAssembler
{
    public static CreateVehicleCommand ToCommandFromResource(CreateVehicleResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateVehicleCommand(resource.Plate, resource.Brand, resource.Model, resource.Capacity,
            resource.Unit, resource.Status, resource.ProviderId);
    }
}
