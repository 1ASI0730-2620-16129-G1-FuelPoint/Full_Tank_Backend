namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;

public record UpdateDriverCommand(
    int Id,
    string Name,
    string LicenseNumber,
    string Phone,
    string Email,
    string Status,
    int? ProviderId);
