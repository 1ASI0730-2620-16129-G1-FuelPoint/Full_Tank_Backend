namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Payload to authenticate a user (login by email).</summary>
public record SignInResource(string Email, string Password);
