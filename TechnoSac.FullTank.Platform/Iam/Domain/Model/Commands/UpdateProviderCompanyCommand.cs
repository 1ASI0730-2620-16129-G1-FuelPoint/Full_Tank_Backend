namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;

/// <summary>Command to update an existing fuel provider company.</summary>
public record UpdateProviderCompanyCommand(
    int Id,
    string Name,
    string Ruc,
    string Address,
    string Phone,
    decimal Rating,
    IEnumerable<string> FuelTypesOffered,
    string Description);
