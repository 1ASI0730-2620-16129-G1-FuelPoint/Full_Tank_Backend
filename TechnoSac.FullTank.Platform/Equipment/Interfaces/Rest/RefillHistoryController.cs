using System.Net.Mime;
using TechnoSac.FullTank.Platform.Equipment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Equipment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest;

[ApiController]
[Authorize("BUYER")]
[Route("api/v1/refill-history")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Refill History endpoints")]
public class RefillHistoryController(
    IRefillHistoryCommandService commandService,
    IRefillHistoryQueryService queryService,
    IEquipmentQueryService equipmentQueryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get refill history (optionally by equipmentId)", OperationId = "GetAllRefillHistory")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<RefillHistoryResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? equipmentId, CancellationToken cancellationToken)
    {
        if (equipmentId is not null && !await OwnsEquipment(equipmentId.Value, cancellationToken))
            return this.Forbidden();

        var items = await queryService.Handle(new GetAllRefillHistoryQuery(equipmentId), cancellationToken);
        items = items.Where(item => this.OwnsBuyerCompany(item.CompanyId));
        return Ok(items.Select(RefillHistoryResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a refill history record by id", OperationId = "GetRefillHistoryById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(RefillHistoryResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetRefillHistoryByIdQuery(id), cancellationToken);
        if (item is not null && !this.OwnsBuyerCompany(item.CompanyId)) return this.Forbidden();
        return EquipmentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(RefillHistoryResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Register a refill history record", OperationId = "CreateRefillHistory")]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(RefillHistoryResource))]
    public async Task<IActionResult> Create([FromBody] CreateRefillHistoryResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(resource.CompanyId)
            || !await OwnsEquipment(resource.EquipmentId, cancellationToken))
            return this.Forbidden();

        var result = await commandService.Handle(
            CreateRefillHistoryCommandFromResourceAssembler.ToCommandFromResource(resource), cancellationToken);
        return EquipmentActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            created => CreatedAtAction(nameof(GetById), new { id = created.Id },
                RefillHistoryResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    private async Task<bool> OwnsEquipment(int equipmentId, CancellationToken cancellationToken)
    {
        var equipment = await equipmentQueryService.Handle(new GetEquipmentByIdQuery(equipmentId), cancellationToken);
        return equipment is not null && this.OwnsBuyerCompany(equipment.CompanyId);
    }
}
