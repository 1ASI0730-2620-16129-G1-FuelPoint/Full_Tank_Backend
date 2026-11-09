namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

public record DeliveryResource(
    int Id,
    int? OrderId,
    int? ProviderId,
    int? DriverId,
    int? VehicleId,
    string Status,
    string OriginLocation,
    string DestinationLocation,
    string? DispatchedAt,
    string? DeliveredAt,
    string? Notes,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
