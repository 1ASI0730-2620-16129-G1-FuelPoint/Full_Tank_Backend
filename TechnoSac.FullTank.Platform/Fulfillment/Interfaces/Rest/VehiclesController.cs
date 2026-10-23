using System.Net.Mime;
using TechnoSac.FullTank.Platform.Fulfillment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Fulfillment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;

namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest;

[ApiController]
[Authorize("PROVIDER")]
[Route("api/v1/vehicles")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Vehicle endpoints")]
public class VehiclesController(
    IVehicleCommandService commandService,
    IVehicleQueryService queryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get vehicles (optionally by providerId)", OperationId = "GetAllVehicles")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<VehicleResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? providerId, CancellationToken cancellationToken)
    {
        providerId = this.CurrentUser().CompanyId;
        var items = await queryService.Handle(new GetAllVehiclesQuery(providerId), cancellationToken);
        return Ok(items.Select(VehicleResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a vehicle by id", OperationId = "GetVehicleById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(VehicleResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetVehicleByIdQuery(id), cancellationToken);
        if (item is not null && !this.OwnsProviderCompany(item.ProviderId)) return this.Forbidden();
        return FulfillmentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(VehicleResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpGet("provider/{providerId:int}")]
    [SwaggerOperation(Summary = "Get vehicles by provider id", OperationId = "GetVehiclesByProvider")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<VehicleResource>))]
    public async Task<IActionResult> GetByProvider(int providerId, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var items = await queryService.Handle(new GetAllVehiclesQuery(providerId), cancellationToken);
        return Ok(items.Select(VehicleResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("provider/{providerId:int}/available")]
    [SwaggerOperation(Summary = "Get available vehicles by provider id", OperationId = "GetAvailableVehiclesByProvider")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<VehicleResource>))]
    public async Task<IActionResult> GetAvailableByProvider(int providerId, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var items = await queryService.Handle(new GetAvailableVehiclesByProviderQuery(providerId), cancellationToken);
        return Ok(items.Select(VehicleResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a vehicle", OperationId = "CreateVehicle")]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(VehicleResource))]
    public async Task<IActionResult> Create([FromBody] CreateVehicleResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(resource.ProviderId)) return this.Forbidden();
        var result = await commandService.Handle(
            CreateVehicleCommandFromResourceAssembler.ToCommandFromResource(resource), cancellationToken);
        return FulfillmentActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            created => CreatedAtAction(nameof(GetById), new { id = created.Id },
                VehicleResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update a vehicle", OperationId = "UpdateVehicle")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(VehicleResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleResource resource,
        CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetVehicleByIdQuery(id), cancellationToken);
        if (item is null)
            return FulfillmentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
                problemDetailsFactory, _ => Ok());
        if (!this.OwnsProviderCompany(item.ProviderId))
            return this.Forbidden();

        // Prevent provider owner reassignment
        if (resource.ProviderId != item.ProviderId)
            resource = resource with { ProviderId = item.ProviderId };

        var result = await commandService.Handle(
            UpdateVehicleCommandFromResourceAssembler.ToCommandFromResource(id, resource), cancellationToken);
        return FulfillmentActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            updated => Ok(VehicleResourceFromEntityAssembler.ToResourceFromEntity(updated)));
    }
}
