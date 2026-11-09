namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

public record UpdateVehicleResource(
    string Plate,
    string Brand,
    string Model,
    int Capacity,
    string Unit,
    string Status,
    int? ProviderId);
