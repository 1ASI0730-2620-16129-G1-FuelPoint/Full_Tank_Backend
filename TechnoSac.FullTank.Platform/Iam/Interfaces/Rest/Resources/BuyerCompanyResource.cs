namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Buyer company representation returned by the API.</summary>
public record BuyerCompanyResource(
    int Id,
    string Name,
    string Ruc,
    string Sector,
    string Address,
    string ContactEmail,
    string Phone,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? RegistrationToken = null);
