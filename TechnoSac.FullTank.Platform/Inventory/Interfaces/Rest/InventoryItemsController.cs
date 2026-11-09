using System.Net.Mime;
using TechnoSac.FullTank.Platform.Inventory.Application.CommandServices;
using TechnoSac.FullTank.Platform.Inventory.Application.QueryServices;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;

namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest;

[ApiController]
[Authorize("BUYER", "PROVIDER")]
[Route("api/v1/inventory-items")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Inventory Item endpoints")]
public class InventoryItemsController(
    IInventoryItemCommandService commandService,
    IInventoryItemQueryService queryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get inventory items (optionally by providerId)", OperationId = "GetAllInventoryItems")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<InventoryItemResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? providerId, CancellationToken cancellationToken)
    {
        var items = await queryService.Handle(new GetAllInventoryItemsQuery(providerId), cancellationToken);
        return Ok(items.Select(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get an inventory item by id", OperationId = "GetInventoryItemById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(InventoryItemResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetInventoryItemByIdQuery(id), cancellationToken);
        if (item is not null && this.CurrentUser().Role == "PROVIDER" && !this.OwnsProviderCompany(item.ProviderId))
            return this.Forbidden();
        return InventoryActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [Authorize("PROVIDER")]
    [SwaggerOperation(Summary = "Create an inventory item", OperationId = "CreateInventoryItem")]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(InventoryItemResource))]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(resource.ProviderId)) return this.Forbidden();
        var result = await commandService.Handle(
            CreateInventoryItemCommandFromResourceAssembler.ToCommandFromResource(resource), cancellationToken);
        return InventoryActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            created => CreatedAtAction(nameof(GetById), new { id = created.Id },
                InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    [HttpPut("{id:int}")]
    [Authorize("PROVIDER")]
    [SwaggerOperation(Summary = "Update an inventory item", OperationId = "UpdateInventoryItem")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(InventoryItemResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInventoryItemResource resource,
        CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetInventoryItemByIdQuery(id), cancellationToken);
        if (item is null)
            return InventoryActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
                problemDetailsFactory, _ => Ok());
        if (!this.OwnsProviderCompany(item.ProviderId))
            return this.Forbidden();

        // Prevent provider owner reassignment
        if (resource.ProviderId != item.ProviderId)
            resource = resource with { ProviderId = item.ProviderId };

        var result = await commandService.Handle(
            UpdateInventoryItemCommandFromResourceAssembler.ToCommandFromResource(id, resource), cancellationToken);
        return InventoryActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            updated => Ok(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(updated)));
    }

    [HttpDelete("{id:int}")]
    [Authorize("PROVIDER")]
    [SwaggerOperation(Summary = "Delete an inventory item", OperationId = "DeleteInventoryItem")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetInventoryItemByIdQuery(id), cancellationToken);
        if (item is not null && !this.OwnsProviderCompany(item.ProviderId)) return this.Forbidden();
        var result = await commandService.Handle(new DeleteInventoryItemCommand(id), cancellationToken);
        return InventoryActionResultAssembler.ToActionResult(this, result, problemDetailsFactory, () => NoContent());
    }
}
