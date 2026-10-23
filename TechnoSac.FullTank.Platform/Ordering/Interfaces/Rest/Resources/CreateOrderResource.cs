namespace TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Resources;

public record CreateOrderResource(
    int? RequestId,
    int? BuyerCompanyId,
    int? ProviderId,
    int? EquipmentId,
    string FuelType,
    int Quantity,
    string Unit,
    decimal UnitPrice,
    decimal TotalAmount,
    string DeliveryAddress,
    string Status = "",
    string PaymentStatus = "",
    int? DriverId = null,
    int? VehicleId = null,
    string? EstimatedDeliveryDate = null,
    string? DispatchedAt = null,
    string? DeliveredAt = null,
    string? PaidAt = null,
    string? ClosedAt = null,
    string? CancelledAt = null,
    string? CancelReason = null);
