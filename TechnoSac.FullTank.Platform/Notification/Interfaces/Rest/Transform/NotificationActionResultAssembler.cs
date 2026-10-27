using TechnoSac.FullTank.Platform.Notification.Domain.Model;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Notification.Interfaces.Rest.Transform;

/// <summary>Maps Notification results/errors to <see cref="IActionResult" /> with localized ProblemDetails.</summary>
public static class NotificationActionResultAssembler
{
    public static int ToStatusCode(NotificationError error)
    {
        return error switch
        {
            NotificationError.NotFound => StatusCodes.Status404NotFound,
            NotificationError.ValidationError => StatusCodes.Status400BadRequest,
            NotificationError.OperationCancelled => StatusCodes.Status409Conflict,
            NotificationError.DatabaseError => StatusCodes.Status500InternalServerError,
            NotificationError.InternalServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }

    public static IActionResult ToActionResult<T>(ControllerBase controller, Result<T> result,
        ProblemDetailsFactory problemDetailsFactory, Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess(result.Value!);
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((NotificationError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResult(ControllerBase controller, Result result,
        ProblemDetailsFactory problemDetailsFactory, Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess();
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((NotificationError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromEntity<T>(ControllerBase controller, T? entity,
        IStringLocalizer<ErrorMessages> errorLocalizer, ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> onSuccess) where T : class
    {
        if (entity is null)
            return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode(NotificationError.NotFound),
                NotificationError.NotFound, errorLocalizer[nameof(NotificationError.NotFound)]);
        return onSuccess(entity);
    }
}
