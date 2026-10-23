namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;

public record UpdateVehicleCommand(
    int Id,
    string Plate,
    string Brand,
    string Model,
    int Capacity,
    string Unit,
    string Status,
    int? ProviderId);
