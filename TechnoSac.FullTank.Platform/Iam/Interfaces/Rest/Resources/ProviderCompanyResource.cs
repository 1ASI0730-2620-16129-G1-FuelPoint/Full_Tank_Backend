namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Provider company representation returned by the API.</summary>
public record ProviderCompanyResource(
    int Id,
    string Name,
    string Ruc,
    string Address,
    string Phone,
    decimal Rating,
    IReadOnlyList<string> FuelTypesOffered,
    string Description,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? RegistrationToken = null);
