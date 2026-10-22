using System.Net.Mime;
using TechnoSac.FullTank.Platform.Inventory.Application.CommandServices;
using TechnoSac.FullTank.Platform.Inventory.Application.QueryServices;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest;

[ApiController]
[Authorize("PROVIDER")]
[Route("api/v1/inventory-movements")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Inventory Movement endpoints")]
public class InventoryMovementsController(
    IInventoryMovementCommandService commandService,
    IInventoryMovementQueryService queryService,
    IInventoryItemQueryService inventoryItemQueryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get inventory movements (optionally by inventoryItemId)",
        OperationId = "GetAllInventoryMovements")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<InventoryMovementResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? inventoryItemId, CancellationToken cancellationToken)
    {
        if (inventoryItemId is not null && !await OwnsInventoryItem(inventoryItemId.Value, cancellationToken))
            return this.Forbidden();

        var items = await queryService.Handle(new GetAllInventoryMovementsQuery(inventoryItemId), cancellationToken);
        items = items.Where(item => this.OwnsProviderCompany(item.ProviderId));
        return Ok(items.Select(InventoryMovementResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get an inventory movement by id", OperationId = "GetInventoryMovementById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(InventoryMovementResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetInventoryMovementByIdQuery(id), cancellationToken);
        if (item is not null && !this.OwnsProviderCompany(item.ProviderId)) return this.Forbidden();
        return InventoryActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory,
            found => Ok(InventoryMovementResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Register an inventory movement", OperationId = "CreateInventoryMovement")]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(InventoryMovementResource))]
    public async Task<IActionResult> Create([FromBody] CreateInventoryMovementResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(resource.ProviderId)
            || resource.InventoryItemId is not int inventoryItemId
            || !await OwnsInventoryItem(inventoryItemId, cancellationToken))
            return this.Forbidden();

        var result = await commandService.Handle(
            CreateInventoryMovementCommandFromResourceAssembler.ToCommandFromResource(resource), cancellationToken);
        return InventoryActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            created => CreatedAtAction(nameof(GetById), new { id = created.Id },
                InventoryMovementResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    private async Task<bool> OwnsInventoryItem(int inventoryItemId, CancellationToken cancellationToken)
    {
        var item = await inventoryItemQueryService.Handle(new GetInventoryItemByIdQuery(inventoryItemId),
            cancellationToken);
        return item is not null && this.OwnsProviderCompany(item.ProviderId);
    }
}
