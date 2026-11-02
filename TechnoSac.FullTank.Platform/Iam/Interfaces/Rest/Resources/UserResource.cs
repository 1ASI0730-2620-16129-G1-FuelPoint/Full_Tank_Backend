namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>User representation returned by the API. Never exposes the password hash.</summary>
public record UserResource(
    int Id,
    string Name,
    string Email,
    string Username,
    string Role,
    int? CompanyId,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
