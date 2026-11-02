namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Payload to update a provider company.</summary>
public record UpdateProviderCompanyResource(
    string Name,
    string Ruc,
    string Address,
    string Phone,
    decimal Rating,
    List<string> FuelTypesOffered,
    string Description);
