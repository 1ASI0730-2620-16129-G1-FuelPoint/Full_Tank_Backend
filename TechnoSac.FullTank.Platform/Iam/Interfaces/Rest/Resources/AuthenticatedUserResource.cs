namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Response returned after a successful sign-in. Never exposes the password hash.</summary>
public record AuthenticatedUserResource(
    int Id,
    string Name,
    string Email,
    string Username,
    string Role,
    int? CompanyId,
    string Token);
