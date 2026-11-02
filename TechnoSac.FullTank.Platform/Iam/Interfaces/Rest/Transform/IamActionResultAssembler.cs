using TechnoSac.FullTank.Platform.Iam.Domain.Model;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>
///     Maps IAM <see cref="Result{T}" /> / <see cref="Result" /> outcomes to <see cref="IActionResult" />,
///     translating <see cref="IamError" /> values into HTTP status codes and localized ProblemDetails.
/// </summary>
public static class IamActionResultAssembler
{
    public static int ToStatusCode(IamError error)
    {
        return error switch
        {
            IamError.InvalidCredentials => StatusCodes.Status401Unauthorized,
            IamError.UserNotFound => StatusCodes.Status404NotFound,
            IamError.BuyerCompanyNotFound => StatusCodes.Status404NotFound,
            IamError.ProviderCompanyNotFound => StatusCodes.Status404NotFound,
            IamError.EmailAlreadyTaken => StatusCodes.Status409Conflict,
            IamError.UsernameAlreadyTaken => StatusCodes.Status409Conflict,
            IamError.RucAlreadyTaken => StatusCodes.Status409Conflict,
            IamError.OperationCancelled => StatusCodes.Status409Conflict,
            IamError.ValidationError => StatusCodes.Status400BadRequest,
            IamError.PasswordChangeFailed => StatusCodes.Status400BadRequest,
            IamError.DatabaseError => StatusCodes.Status500InternalServerError,
            IamError.InternalServerError => StatusCodes.Status500InternalServerError,
            IamError.ExternalServiceError => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status400BadRequest
        };
    }

    /// <summary>Maps a <see cref="Result{T}" /> to an action result (success returns the value).</summary>
    public static IActionResult ToActionResult<T>(
        ControllerBase controller,
        Result<T> result,
        ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess(result.Value!);
        var statusCode = ToStatusCode((IamError)result.Error!);
        return problemDetailsFactory.CreateProblemDetails(controller, statusCode, result.Error, result.Message);
    }

    /// <summary>Maps a non-generic <see cref="Result" /> to an action result.</summary>
    public static IActionResult ToActionResult(
        ControllerBase controller,
        Result result,
        ProblemDetailsFactory problemDetailsFactory,
        Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess();
        var statusCode = ToStatusCode((IamError)result.Error!);
        return problemDetailsFactory.CreateProblemDetails(controller, statusCode, result.Error, result.Message);
    }

    /// <summary>Maps a nullable entity (query result) to an action result, returning a 404 when null.</summary>
    public static IActionResult ToActionResultFromEntity<T>(
        ControllerBase controller,
        T? entity,
        IamError notFoundError,
        IStringLocalizer<ErrorMessages> errorLocalizer,
        ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> onSuccess) where T : class
    {
        if (entity is null)
            return problemDetailsFactory.CreateProblemDetails(
                controller,
                ToStatusCode(notFoundError),
                notFoundError,
                errorLocalizer[notFoundError.ToString()]);
        return onSuccess(entity);
    }
}
