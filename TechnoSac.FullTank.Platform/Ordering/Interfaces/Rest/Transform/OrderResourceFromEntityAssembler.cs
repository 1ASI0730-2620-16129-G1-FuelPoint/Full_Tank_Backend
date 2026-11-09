using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Transform;

public static class OrderResourceFromEntityAssembler
{
    public static OrderResource ToResourceFromEntity(Order entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new OrderResource(entity.Id, entity.RequestId, entity.BuyerCompanyId, entity.ProviderId,
            entity.EquipmentId, entity.FuelType, entity.Quantity, entity.Unit, entity.UnitPrice, entity.TotalAmount,
            entity.DeliveryAddress, entity.Status, entity.PaymentStatus, entity.DriverId, entity.VehicleId,
            entity.EstimatedDeliveryDate, entity.DispatchedAt, entity.DeliveredAt, entity.PaidAt, entity.ClosedAt,
            entity.CancelledAt, entity.CancelReason, entity.CreatedAt, entity.UpdatedAt);
    }
}
