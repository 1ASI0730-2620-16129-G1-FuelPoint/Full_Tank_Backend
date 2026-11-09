namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Payload to register a new user. Username is optional and defaults to the email.</summary>
public record SignUpResource(
    string Name,
    string Email,
    string Password,
    string Role,
    int? CompanyId = null,
    string Username = "",
    string? RegistrationToken = null);
