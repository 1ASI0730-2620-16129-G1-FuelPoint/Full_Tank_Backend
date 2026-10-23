using TechnoSac.FullTank.Platform.Ordering.Domain.Model;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Transform;

/// <summary>Maps Ordering results/errors to <see cref="IActionResult" /> with localized ProblemDetails.</summary>
public static class OrderingActionResultAssembler
{
    public static int ToStatusCode(OrderingError error)
    {
        return error switch
        {
            OrderingError.NotFound => StatusCodes.Status404NotFound,
            OrderingError.ValidationError => StatusCodes.Status400BadRequest,
            OrderingError.OperationCancelled => StatusCodes.Status409Conflict,
            OrderingError.DatabaseError => StatusCodes.Status500InternalServerError,
            OrderingError.InternalServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }

    public static IActionResult ToActionResult<T>(ControllerBase controller, Result<T> result,
        ProblemDetailsFactory problemDetailsFactory, Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess(result.Value!);
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((OrderingError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResult(ControllerBase controller, Result result,
        ProblemDetailsFactory problemDetailsFactory, Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess();
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((OrderingError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromEntity<T>(ControllerBase controller, T? entity,
        IStringLocalizer<ErrorMessages> errorLocalizer, ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> onSuccess) where T : class
    {
        if (entity is null)
            return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode(OrderingError.NotFound),
                OrderingError.NotFound, errorLocalizer[nameof(OrderingError.NotFound)]);
        return onSuccess(entity);
    }
}
