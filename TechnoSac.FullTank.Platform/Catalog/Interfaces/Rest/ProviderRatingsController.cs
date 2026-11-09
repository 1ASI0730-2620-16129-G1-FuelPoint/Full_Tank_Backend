using System.Net.Mime;
using TechnoSac.FullTank.Platform.Catalog.Application.CommandServices;
using TechnoSac.FullTank.Platform.Catalog.Application.QueryServices;
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
[Route("api/v1/provider-ratings")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Provider Rating endpoints")]
public class ProviderRatingsController(
    IProviderRatingCommandService commandService,
    IProviderRatingQueryService queryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get provider ratings", OperationId = "GetAllProviderRatings")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<ProviderRatingResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? companyId, [FromQuery] int? providerId,
        CancellationToken cancellationToken)
    {
        companyId = this.CurrentUser().CompanyId;
        var items = await queryService.Handle(new GetAllProviderRatingsQuery(companyId, providerId),
            cancellationToken);
        return Ok(items.Select(ProviderRatingResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a provider rating by id", OperationId = "GetProviderRatingById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ProviderRatingResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetProviderRatingByIdQuery(id), cancellationToken);
        if (item is not null && !this.OwnsBuyerCompany(item.CompanyId)) return this.Forbidden();
        return CatalogActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(ProviderRatingResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a provider rating", OperationId = "CreateProviderRating")]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(ProviderRatingResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid or duplicate rating")]
    public async Task<IActionResult> Create([FromBody] CreateProviderRatingResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(resource.CompanyId)) return this.Forbidden();
        var result = await commandService.Handle(
            CreateProviderRatingCommandFromResourceAssembler.ToCommandFromResource(resource), cancellationToken);
        return CatalogActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            created => CreatedAtAction(nameof(GetById), new { id = created.Id },
                ProviderRatingResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update a provider rating", OperationId = "UpdateProviderRating")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ProviderRatingResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid or duplicate rating")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProviderRatingResource resource,
        CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetProviderRatingByIdQuery(id), cancellationToken);
        if (item is null)
            return CatalogActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
                problemDetailsFactory, _ => Ok());
        if (!this.OwnsBuyerCompany(item.CompanyId))
            return this.Forbidden();

        // Prevent tenant / related id reassignment
        if (resource.CompanyId != item.CompanyId || resource.ProviderId != item.ProviderId)
            resource = resource with { CompanyId = item.CompanyId, ProviderId = item.ProviderId };

        var result = await commandService.Handle(
            UpdateProviderRatingCommandFromResourceAssembler.ToCommandFromResource(id, resource), cancellationToken);
        return CatalogActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            updated => Ok(ProviderRatingResourceFromEntityAssembler.ToResourceFromEntity(updated)));
    }
}
