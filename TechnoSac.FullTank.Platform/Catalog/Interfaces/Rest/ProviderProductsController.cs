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
[Authorize("BUYER", "PROVIDER")]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Provider Product endpoints")]
public class ProviderProductsController(
    IProviderProductCommandService commandService,
    IProviderProductQueryService queryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get all provider products", OperationId = "GetAllProviderProducts")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<ProviderProductResource>))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await queryService.Handle(new GetAllProviderProductsQuery(), cancellationToken);
        return Ok(items.Select(ProviderProductResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a provider product by id", OperationId = "GetProviderProductById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ProviderProductResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetProviderProductByIdQuery(id), cancellationToken);
        return CatalogActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer, problemDetailsFactory,
            found => Ok(ProviderProductResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [Authorize("PROVIDER")]
    [SwaggerOperation(Summary = "Create a provider product", OperationId = "CreateProviderProduct")]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(ProviderProductResource))]
    public async Task<IActionResult> Create([FromBody] CreateProviderProductResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(resource.ProviderId)) return this.Forbidden();
        var result = await commandService.Handle(
            CreateProviderProductCommandFromResourceAssembler.ToCommandFromResource(resource), cancellationToken);
        return CatalogActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            created => CreatedAtAction(nameof(GetById), new { id = created.Id },
                ProviderProductResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    [HttpPut("{id:int}")]
    [Authorize("PROVIDER")]
    [SwaggerOperation(Summary = "Update a provider product", OperationId = "UpdateProviderProduct")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ProviderProductResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProviderProductResource resource,
        CancellationToken cancellationToken)
    {
        var product = await queryService.Handle(new GetProviderProductByIdQuery(id), cancellationToken);
        if (product is null)
            return CatalogActionResultAssembler.ToActionResultFromEntity(this, product, errorLocalizer,
                problemDetailsFactory, _ => Ok());
        if (!this.OwnsProviderCompany(product.ProviderId))
            return this.Forbidden();

        // Prevent provider owner reassignment
        if (resource.ProviderId != product.ProviderId)
            resource = resource with { ProviderId = product.ProviderId };

        var result = await commandService.Handle(
            UpdateProviderProductCommandFromResourceAssembler.ToCommandFromResource(id, resource), cancellationToken);
        return CatalogActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            updated => Ok(ProviderProductResourceFromEntityAssembler.ToResourceFromEntity(updated)));
    }

    [HttpDelete("{id:int}")]
    [Authorize("PROVIDER")]
    [SwaggerOperation(Summary = "Delete a provider product", OperationId = "DeleteProviderProduct")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var product = await queryService.Handle(new GetProviderProductByIdQuery(id), cancellationToken);
        if (product is not null && !this.OwnsProviderCompany(product.ProviderId)) return this.Forbidden();
        var result = await commandService.Handle(new DeleteProviderProductCommand(id), cancellationToken);
        return CatalogActionResultAssembler.ToActionResult(this, result, problemDetailsFactory, () => NoContent());
    }
}
