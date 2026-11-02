namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Payload to update a buyer company.</summary>
public record UpdateBuyerCompanyResource(
    string Name,
    string Ruc,
    string Sector,
    string Address,
    string ContactEmail,
    string Phone);
