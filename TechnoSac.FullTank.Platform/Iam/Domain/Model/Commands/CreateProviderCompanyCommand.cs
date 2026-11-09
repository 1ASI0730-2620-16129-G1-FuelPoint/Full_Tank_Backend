namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;

/// <summary>Command to create a fuel provider company.</summary>
public record CreateProviderCompanyCommand(
    string Name,
    string Ruc,
    string Address,
    string Phone,
    decimal Rating,
    IEnumerable<string> FuelTypesOffered,
    string Description);
