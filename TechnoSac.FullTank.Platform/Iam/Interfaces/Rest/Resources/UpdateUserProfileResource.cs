namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Payload to update a user's editable profile fields.</summary>
public record UpdateUserProfileResource(string Name, string Email);
