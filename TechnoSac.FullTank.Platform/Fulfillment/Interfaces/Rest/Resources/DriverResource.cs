namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

public record DriverResource(
    int Id,
    string Name,
    string LicenseNumber,
    string Phone,
    string Email,
    string Status,
    int? ProviderId,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
