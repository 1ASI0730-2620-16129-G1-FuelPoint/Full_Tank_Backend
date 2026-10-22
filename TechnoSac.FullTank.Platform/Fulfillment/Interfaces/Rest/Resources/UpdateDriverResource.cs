namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

public record UpdateDriverResource(
    string Name,
    string LicenseNumber,
    string Phone,
    string Email,
    string Status,
    int? ProviderId);
