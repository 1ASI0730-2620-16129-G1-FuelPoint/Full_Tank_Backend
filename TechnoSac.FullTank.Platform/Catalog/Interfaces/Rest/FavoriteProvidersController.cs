using System.Net.Mime;
using TechnoSac.FullTank.Platform.Catalog.Application.CommandServices;
using TechnoSac.FullTank.Platform.Catalog.Application.QueryServices;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;

namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest;

[ApiController]
[Authorize("BUYER")]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Favorite Provider endpoints")]
public class FavoriteProvidersController(
    IFavoriteProviderCommandService commandService,
    IFavoriteProviderQueryService queryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get favorite providers (optionally by companyId)", OperationId = "GetAllFavoriteProviders")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<FavoriteProviderResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? companyId, CancellationToken cancellationToken)
    {
        companyId = this.CurrentUser().CompanyId;
        var items = await queryService.Handle(new GetAllFavoriteProvidersQuery(companyId), cancellationToken);
        return Ok(items.Select(FavoriteProviderResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a favorite provider by id", OperationId = "GetFavoriteProviderById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(FavoriteProviderResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetFavoriteProviderByIdQuery(id), cancellationToken);
        if (item is not null && !this.OwnsBuyerCompany(item.CompanyId)) return this.Forbidden();
        return CatalogActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer, problemDetailsFactory,
            found => Ok(FavoriteProviderResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Add a favorite provider", OperationId = "CreateFavoriteProvider")]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(FavoriteProviderResource))]
    public async Task<IActionResult> Create([FromBody] CreateFavoriteProviderResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(resource.CompanyId)) return this.Forbidden();
        var result = await commandService.Handle(
            CreateFavoriteProviderCommandFromResourceAssembler.ToCommandFromResource(resource), cancellationToken);
        return CatalogActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            created => CreatedAtAction(nameof(GetById), new { id = created.Id },
                FavoriteProviderResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Remove a favorite provider", OperationId = "DeleteFavoriteProvider")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var favorite = await queryService.Handle(new GetFavoriteProviderByIdQuery(id), cancellationToken);
        if (favorite is not null && !this.OwnsBuyerCompany(favorite.CompanyId)) return this.Forbidden();
        var result = await commandService.Handle(new DeleteFavoriteProviderCommand(id), cancellationToken);
        return CatalogActionResultAssembler.ToActionResult(this, result, problemDetailsFactory, () => NoContent());
    }
}
