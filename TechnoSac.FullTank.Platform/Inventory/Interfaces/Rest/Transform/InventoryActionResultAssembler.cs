using TechnoSac.FullTank.Platform.Inventory.Domain.Model;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Transform;

/// <summary>Maps Inventory results/errors to <see cref="IActionResult" /> with localized ProblemDetails.</summary>
public static class InventoryActionResultAssembler
{
    public static int ToStatusCode(InventoryError error)
    {
        return error switch
        {
            InventoryError.NotFound => StatusCodes.Status404NotFound,
            InventoryError.ValidationError => StatusCodes.Status400BadRequest,
            InventoryError.OperationCancelled => StatusCodes.Status409Conflict,
            InventoryError.DatabaseError => StatusCodes.Status500InternalServerError,
            InventoryError.InternalServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }

    public static IActionResult ToActionResult<T>(ControllerBase controller, Result<T> result,
        ProblemDetailsFactory problemDetailsFactory, Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess(result.Value!);
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((InventoryError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResult(ControllerBase controller, Result result,
        ProblemDetailsFactory problemDetailsFactory, Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess();
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((InventoryError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromEntity<T>(ControllerBase controller, T? entity,
        IStringLocalizer<ErrorMessages> errorLocalizer, ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> onSuccess) where T : class
    {
        if (entity is null)
            return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode(InventoryError.NotFound),
                InventoryError.NotFound, errorLocalizer[nameof(InventoryError.NotFound)]);
        return onSuccess(entity);
    }
}
