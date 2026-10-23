namespace TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Resources;

public record UpdateOrderResource(
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
    string Status,
    string PaymentStatus,
    int? DriverId,
    int? VehicleId,
    string? EstimatedDeliveryDate,
    string? DispatchedAt,
    string? DeliveredAt,
    string? PaidAt,
    string? ClosedAt,
    string? CancelledAt,
    string? CancelReason);
