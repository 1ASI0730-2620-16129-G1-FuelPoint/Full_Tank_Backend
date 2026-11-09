using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Transform;

public static class DeliveryResourceFromEntityAssembler
{
    public static DeliveryResource ToResourceFromEntity(Delivery entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new DeliveryResource(entity.Id, entity.OrderId, entity.ProviderId, entity.DriverId, entity.VehicleId,
            entity.Status, entity.OriginLocation, entity.DestinationLocation, entity.DispatchedAt, entity.DeliveredAt,
            entity.Notes, entity.CreatedAt, entity.UpdatedAt);
    }
}
