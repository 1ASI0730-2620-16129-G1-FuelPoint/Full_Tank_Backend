namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;

/// <summary>Command to register a new user. <paramref name="Username" /> defaults to the email when empty.</summary>
public record SignUpCommand(
    string Name,
    string Email,
    string Username,
    string Password,
    string Role,
    int? CompanyId,
    string? RegistrationToken = null);
