namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

public record CreateDeliveryResource(
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
