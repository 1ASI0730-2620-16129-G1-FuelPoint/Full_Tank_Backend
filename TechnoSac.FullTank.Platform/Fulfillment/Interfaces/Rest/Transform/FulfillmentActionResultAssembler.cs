using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Transform;

/// <summary>Maps Fulfillment results/errors to <see cref="IActionResult" /> with localized ProblemDetails.</summary>
public static class FulfillmentActionResultAssembler
{
    public static int ToStatusCode(FulfillmentError error)
    {
        return error switch
        {
            FulfillmentError.NotFound => StatusCodes.Status404NotFound,
            FulfillmentError.ValidationError => StatusCodes.Status400BadRequest,
            FulfillmentError.OperationCancelled => StatusCodes.Status409Conflict,
            FulfillmentError.DatabaseError => StatusCodes.Status500InternalServerError,
            FulfillmentError.InternalServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }

    public static IActionResult ToActionResult<T>(ControllerBase controller, Result<T> result,
        ProblemDetailsFactory problemDetailsFactory, Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess(result.Value!);
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((FulfillmentError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResult(ControllerBase controller, Result result,
        ProblemDetailsFactory problemDetailsFactory, Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess();
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((FulfillmentError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromEntity<T>(ControllerBase controller, T? entity,
        IStringLocalizer<ErrorMessages> errorLocalizer, ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> onSuccess) where T : class
    {
        if (entity is null)
            return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode(FulfillmentError.NotFound),
                FulfillmentError.NotFound, errorLocalizer[nameof(FulfillmentError.NotFound)]);
        return onSuccess(entity);
    }
}
