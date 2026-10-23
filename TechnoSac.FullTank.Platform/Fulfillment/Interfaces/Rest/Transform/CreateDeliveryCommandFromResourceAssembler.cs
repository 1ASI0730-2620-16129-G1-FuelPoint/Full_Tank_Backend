using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Transform;

public static class CreateDeliveryCommandFromResourceAssembler
{
    public static CreateDeliveryCommand ToCommandFromResource(CreateDeliveryResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateDeliveryCommand(resource.OrderId, resource.ProviderId, resource.DriverId, resource.VehicleId,
            resource.Status, resource.OriginLocation, resource.DestinationLocation, resource.DispatchedAt,
            resource.DeliveredAt, resource.Notes);
    }
}
