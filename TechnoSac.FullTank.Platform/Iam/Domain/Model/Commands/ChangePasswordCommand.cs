namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;

/// <summary>Command to change a user's password after verifying the current one.</summary>
public record ChangePasswordCommand(int UserId, string CurrentPassword, string NewPassword);
