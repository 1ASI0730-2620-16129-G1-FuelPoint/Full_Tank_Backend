namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Payload to create a buyer company.</summary>
public record CreateBuyerCompanyResource(
    string Name,
    string Ruc,
    string Sector,
    string Address,
    string ContactEmail,
    string Phone);
