namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;

public record CreateDriverCommand(
    string Name,
    string LicenseNumber,
    string Phone,
    string Email,
    string Status,
    int? ProviderId);
