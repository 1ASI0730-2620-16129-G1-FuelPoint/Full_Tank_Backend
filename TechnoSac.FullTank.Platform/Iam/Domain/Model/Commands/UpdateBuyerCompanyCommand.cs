namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;

/// <summary>Command to update an existing buyer company.</summary>
public record UpdateBuyerCompanyCommand(
    int Id,
    string Name,
    string Ruc,
    string Sector,
    string Address,
    string ContactEmail,
    string Phone);
