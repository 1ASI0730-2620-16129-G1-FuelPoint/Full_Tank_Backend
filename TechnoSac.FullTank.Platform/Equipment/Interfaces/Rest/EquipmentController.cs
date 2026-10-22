using System.Net.Mime;
using TechnoSac.FullTank.Platform.Equipment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Equipment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;

namespace TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest;

[ApiController]
[Authorize("BUYER")]
[Route("api/v1/equipment")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Equipment endpoints")]
public class EquipmentController(
    IEquipmentCommandService commandService,
    IEquipmentQueryService queryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get equipment (optionally by companyId)", OperationId = "GetAllEquipment")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<EquipmentResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? companyId, CancellationToken cancellationToken)
    {
        companyId = this.CurrentUser().CompanyId;
        var items = await queryService.Handle(new GetAllEquipmentQuery(companyId), cancellationToken);
        return Ok(items.Select(EquipmentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get an equipment by id", OperationId = "GetEquipmentById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(EquipmentResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetEquipmentByIdQuery(id), cancellationToken);
        if (item is not null && !this.OwnsBuyerCompany(item.CompanyId)) return this.Forbidden();
        return EquipmentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(EquipmentResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create an equipment", OperationId = "CreateEquipment")]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(EquipmentResource))]
    public async Task<IActionResult> Create([FromBody] CreateEquipmentResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(resource.CompanyId)) return this.Forbidden();
        var result = await commandService.Handle(
            CreateEquipmentCommandFromResourceAssembler.ToCommandFromResource(resource), cancellationToken);
        return EquipmentActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            created => CreatedAtAction(nameof(GetById), new { id = created.Id },
                EquipmentResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update an equipment", OperationId = "UpdateEquipment")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(EquipmentResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEquipmentResource resource,
        CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetEquipmentByIdQuery(id), cancellationToken);
        if (item is null)
            return EquipmentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
                problemDetailsFactory, _ => Ok());
        if (!this.OwnsBuyerCompany(item.CompanyId))
            return this.Forbidden();

        // Prevent tenant/owner reassignment
        if (resource.CompanyId != item.CompanyId)
            resource = resource with { CompanyId = item.CompanyId };

        var result = await commandService.Handle(
            UpdateEquipmentCommandFromResourceAssembler.ToCommandFromResource(id, resource), cancellationToken);
        return EquipmentActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            updated => Ok(EquipmentResourceFromEntityAssembler.ToResourceFromEntity(updated)));
    }
}
