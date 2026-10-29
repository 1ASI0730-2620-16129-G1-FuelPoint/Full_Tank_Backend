namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;

/// <summary>Command to update the editable profile fields of a user. Never touches the password.</summary>
public record UpdateUserProfileCommand(int UserId, string Name, string Email);
