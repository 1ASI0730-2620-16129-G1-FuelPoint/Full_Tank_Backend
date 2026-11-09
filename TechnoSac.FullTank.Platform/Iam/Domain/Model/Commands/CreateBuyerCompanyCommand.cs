namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;

/// <summary>Command to create a buyer company.</summary>
public record CreateBuyerCompanyCommand(
    string Name,
    string Ruc,
    string Sector,
    string Address,
    string ContactEmail,
    string Phone);
