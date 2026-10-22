using TechnoSac.FullTank.Platform.Catalog.Domain.Model;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Transform;

/// <summary>Maps Catalog results/errors to <see cref="IActionResult" /> with localized ProblemDetails.</summary>
public static class CatalogActionResultAssembler
{
    public static int ToStatusCode(CatalogError error)
    {
        return error switch
        {
            CatalogError.NotFound => StatusCodes.Status404NotFound,
            CatalogError.ValidationError => StatusCodes.Status400BadRequest,
            CatalogError.OperationCancelled => StatusCodes.Status409Conflict,
            CatalogError.DatabaseError => StatusCodes.Status500InternalServerError,
            CatalogError.InternalServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }

    public static IActionResult ToActionResult<T>(ControllerBase controller, Result<T> result,
        ProblemDetailsFactory problemDetailsFactory, Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess(result.Value!);
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((CatalogError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResult(ControllerBase controller, Result result,
        ProblemDetailsFactory problemDetailsFactory, Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess) return onSuccess();
        return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode((CatalogError)result.Error!),
            result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromEntity<T>(ControllerBase controller, T? entity,
        IStringLocalizer<ErrorMessages> errorLocalizer, ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> onSuccess) where T : class
    {
        if (entity is null)
            return problemDetailsFactory.CreateProblemDetails(controller, ToStatusCode(CatalogError.NotFound),
                CatalogError.NotFound, errorLocalizer[nameof(CatalogError.NotFound)]);
        return onSuccess(entity);
    }
}
