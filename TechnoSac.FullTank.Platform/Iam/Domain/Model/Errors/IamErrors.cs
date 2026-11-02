using TechnoSac.FullTank.Platform.Shared.Domain.Model;

namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Errors;

/// <summary>
///     Typed catalog of IAM domain errors (codes + default messages).
/// </summary>
/// <remarks>
///     Complements the <see cref="IamError" /> enum used for HTTP status mapping and localization.
/// </remarks>
public static class IamErrors
{
    public static readonly Error InvalidCredentials = new("Iam.InvalidCredentials", "Invalid email or password.");

    public static readonly Error EmailAlreadyTaken =
        new("Iam.EmailAlreadyTaken", "The specified email is already registered.");

    public static readonly Error UsernameAlreadyTaken =
        new("Iam.UsernameAlreadyTaken", "The specified username is already taken.");

    public static readonly Error UserCreationFailed =
        new("Iam.UserCreationFailed", "An error occurred while creating the user.");

    public static readonly Error RucAlreadyTaken =
        new("Iam.RucAlreadyTaken", "The specified RUC is already registered.");
}
