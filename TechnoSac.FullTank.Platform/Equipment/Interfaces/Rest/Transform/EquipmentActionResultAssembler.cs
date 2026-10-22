using TechnoSac.FullTank.Platform.Equipment.Domain.Model;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Transform;

/// <summary>Maps Equipment results/errors to <see cref="IActionResult" /> with localized ProblemDetails.</summary>
public static class EquipmentActionResultAssembler
{
    public static int ToStatusCode(EquipmentError error)
    {
        return error switch
        {
            EquipmentError.NotFound => StatusCodes.Status404NotFound,
            EquipmentError.ValidationError => StatusCodes.Status400BadRequest,
            EquipmentError.OperationCancelled => StatusCodes.Status409Conflict,
            EquipmentError.DatabaseError => StatusCodes.Status500InternalServerError,
            EquipmentError.InternalServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }

    public static IActionResult ToActionResult<T>(ControllerBase controller, Result<T> result,
        ProblemDetailsFactory problemDetailsFactory, Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess(result.Value!);
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((EquipmentError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResult(ControllerBase controller, Result result,
        ProblemDetailsFactory problemDetailsFactory, Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess();
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((EquipmentError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromEntity<T>(ControllerBase controller, T? entity,
        IStringLocalizer<ErrorMessages> errorLocalizer, ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> onSuccess) where T : class
    {
        if (entity is null)
            return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode(EquipmentError.NotFound),
                EquipmentError.NotFound, errorLocalizer[nameof(EquipmentError.NotFound)]);
        return onSuccess(entity);
    }
}
