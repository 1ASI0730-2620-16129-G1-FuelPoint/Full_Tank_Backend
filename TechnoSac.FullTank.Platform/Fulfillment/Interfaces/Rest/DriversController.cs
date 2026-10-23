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
[Route("api/v1/drivers")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Driver endpoints")]
public class DriversController(
    IDriverCommandService commandService,
    IDriverQueryService queryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get drivers (optionally by providerId)", OperationId = "GetAllDrivers")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<DriverResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? providerId, CancellationToken cancellationToken)
    {
        providerId = this.CurrentUser().CompanyId;
        var items = await queryService.Handle(new GetAllDriversQuery(providerId), cancellationToken);
        return Ok(items.Select(DriverResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a driver by id", OperationId = "GetDriverById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(DriverResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetDriverByIdQuery(id), cancellationToken);
        if (item is not null && !this.OwnsProviderCompany(item.ProviderId)) return this.Forbidden();
        return FulfillmentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(DriverResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpGet("provider/{providerId:int}")]
    [SwaggerOperation(Summary = "Get drivers by provider id", OperationId = "GetDriversByProvider")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<DriverResource>))]
    public async Task<IActionResult> GetByProvider(int providerId, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var items = await queryService.Handle(new GetAllDriversQuery(providerId), cancellationToken);
        return Ok(items.Select(DriverResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a driver", OperationId = "CreateDriver")]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(DriverResource))]
    public async Task<IActionResult> Create([FromBody] CreateDriverResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(resource.ProviderId)) return this.Forbidden();
        var result = await commandService.Handle(
            CreateDriverCommandFromResourceAssembler.ToCommandFromResource(resource), cancellationToken);
        return FulfillmentActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            created => CreatedAtAction(nameof(GetById), new { id = created.Id },
                DriverResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update a driver", OperationId = "UpdateDriver")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(DriverResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDriverResource resource,
        CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetDriverByIdQuery(id), cancellationToken);
        if (item is null)
            return FulfillmentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
                problemDetailsFactory, _ => Ok());
        if (!this.OwnsProviderCompany(item.ProviderId))
            return this.Forbidden();

        // Prevent provider owner reassignment
        if (resource.ProviderId != item.ProviderId)
            resource = resource with { ProviderId = item.ProviderId };

        var result = await commandService.Handle(
            UpdateDriverCommandFromResourceAssembler.ToCommandFromResource(id, resource), cancellationToken);
        return FulfillmentActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            updated => Ok(DriverResourceFromEntityAssembler.ToResourceFromEntity(updated)));
    }
}
