namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;

public record CreateDeliveryCommand(
    int? OrderId,
    int? ProviderId,
    int? DriverId,
    int? VehicleId,
    string Status,
    string OriginLocation,
    string DestinationLocation,
    string? DispatchedAt,
    string? DeliveredAt,
    string? Notes);
