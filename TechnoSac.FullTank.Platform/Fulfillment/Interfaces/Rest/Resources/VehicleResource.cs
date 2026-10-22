namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

public record VehicleResource(
    int Id,
    string Plate,
    string Brand,
    string Model,
    int Capacity,
    string Unit,
    string Status,
    int? ProviderId,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
