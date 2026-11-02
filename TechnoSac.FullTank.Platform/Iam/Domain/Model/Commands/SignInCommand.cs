namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;

/// <summary>Command to authenticate a user by email and password.</summary>
public record SignInCommand(string Email, string Password);
