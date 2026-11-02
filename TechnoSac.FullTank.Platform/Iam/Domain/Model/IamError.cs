namespace TechnoSac.FullTank.Platform.Iam.Domain.Model;

/// <summary>
///     Enumerates the domain/application errors for the IAM bounded context.
/// </summary>
/// <remarks>
///     Each member name is also the resource key used to localize the error message
///     (see <c>ErrorMessages.resx</c>).
/// </remarks>
public enum IamError
{
    None,
    UserNotFound,
    EmailAlreadyTaken,
    UsernameAlreadyTaken,
    InvalidCredentials,
    PasswordChangeFailed,
    BuyerCompanyNotFound,
    ProviderCompanyNotFound,
    RucAlreadyTaken,
    ValidationError,
    OperationCancelled,
    DatabaseError,
    InternalServerError,
    ExternalServiceError
}
