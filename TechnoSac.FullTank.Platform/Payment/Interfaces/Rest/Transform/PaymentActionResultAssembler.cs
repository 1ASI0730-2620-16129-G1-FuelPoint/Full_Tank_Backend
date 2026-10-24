using TechnoSac.FullTank.Platform.Payment.Domain.Model;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Transform;

/// <summary>Maps Payment results/errors to <see cref="IActionResult" /> with localized ProblemDetails.</summary>
public static class PaymentActionResultAssembler
{
    public static int ToStatusCode(PaymentError error)
    {
        return error switch
        {
            PaymentError.NotFound => StatusCodes.Status404NotFound,
            PaymentError.ValidationError => StatusCodes.Status400BadRequest,
            PaymentError.OperationCancelled => StatusCodes.Status409Conflict,
            PaymentError.DatabaseError => StatusCodes.Status500InternalServerError,
            PaymentError.InternalServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }

    public static IActionResult ToActionResult<T>(ControllerBase controller, Result<T> result,
        ProblemDetailsFactory problemDetailsFactory, Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess(result.Value!);
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((PaymentError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResult(ControllerBase controller, Result result,
        ProblemDetailsFactory problemDetailsFactory, Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess();
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((PaymentError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromEntity<T>(ControllerBase controller, T? entity,
        IStringLocalizer<ErrorMessages> errorLocalizer, ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> onSuccess) where T : class
    {
        if (entity is null)
            return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode(PaymentError.NotFound),
                PaymentError.NotFound, errorLocalizer[nameof(PaymentError.NotFound)]);
        return onSuccess(entity);
    }
}
