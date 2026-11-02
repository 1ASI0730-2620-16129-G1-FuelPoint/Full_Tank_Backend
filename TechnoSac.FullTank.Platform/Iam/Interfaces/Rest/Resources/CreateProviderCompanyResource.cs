namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Payload to create a provider company.</summary>
public record CreateProviderCompanyResource(
    string Name,
    string Ruc,
    string Address,
    string Phone,
    decimal Rating,
    List<string> FuelTypesOffered,
    string Description);
