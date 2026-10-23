using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Transform;

public static class CreateOrderCommandFromResourceAssembler
{
    public static CreateOrderCommand ToCommandFromResource(CreateOrderResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateOrderCommand(resource.RequestId, resource.BuyerCompanyId, resource.ProviderId,
            resource.EquipmentId, resource.FuelType, resource.Quantity, resource.Unit, resource.UnitPrice,
            resource.TotalAmount, resource.DeliveryAddress, resource.Status, resource.PaymentStatus, resource.DriverId,
            resource.VehicleId, resource.EstimatedDeliveryDate, resource.DispatchedAt, resource.DeliveredAt,
            resource.PaidAt, resource.ClosedAt, resource.CancelledAt, resource.CancelReason);
    }
}
