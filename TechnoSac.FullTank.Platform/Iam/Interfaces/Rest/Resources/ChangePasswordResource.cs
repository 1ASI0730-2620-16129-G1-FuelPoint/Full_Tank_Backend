namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Payload to change a user's password.</summary>
public record ChangePasswordResource(string CurrentPassword, string NewPassword);
