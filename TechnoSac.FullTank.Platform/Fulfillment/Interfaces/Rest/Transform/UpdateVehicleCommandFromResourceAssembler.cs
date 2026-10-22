using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Transform;

public static class UpdateVehicleCommandFromResourceAssembler
{
    public static UpdateVehicleCommand ToCommandFromResource(int id, UpdateVehicleResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateVehicleCommand(id, resource.Plate, resource.Brand, resource.Model, resource.Capacity,
            resource.Unit, resource.Status, resource.ProviderId);
    }
}
