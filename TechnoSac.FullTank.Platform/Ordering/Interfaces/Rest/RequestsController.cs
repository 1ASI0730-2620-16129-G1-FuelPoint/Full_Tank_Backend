using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;
using TechnoSac.FullTank.Platform.Ordering.Application.CommandServices;
using TechnoSac.FullTank.Platform.Ordering.Application.QueryServices;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;

namespace TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest;

[ApiController]
[Route("api/v1/fuel-requests")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Fuel Request endpoints")]
[Authorize("BUYER", "PROVIDER")]
public class RequestsController(
    IRequestCommandService commandService,
    IRequestQueryService queryService,
    IOrderCommandService orderCommandService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get fuel requests (optionally by buyerCompanyId or providerId)",
        OperationId = "GetAllRequests")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<RequestResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? buyerCompanyId, [FromQuery] int? providerId,
        CancellationToken cancellationToken)
    {
        var user = this.CurrentUser();
        buyerCompanyId = user.Role == "BUYER" ? user.CompanyId : null;
        providerId = user.Role == "PROVIDER" ? user.CompanyId : null;
        var items = await queryService.Handle(new GetAllRequestsQuery(buyerCompanyId, providerId), cancellationToken);
        return Ok(items.Select(RequestResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a fuel request by id", OperationId = "GetRequestById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(RequestResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetRequestByIdQuery(id), cancellationToken);
        if (item is not null
            && !this.OwnsBuyerCompany(item.BuyerCompanyId)
            && !this.OwnsProviderCompany(item.ProviderId))
            return this.Forbidden();
        return OrderingActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer, problemDetailsFactory,
            found => Ok(RequestResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [Authorize("BUYER")]
    [SwaggerOperation(Summary = "Create a fuel request", OperationId = "CreateRequest")]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(RequestResource))]
    public async Task<IActionResult> Create([FromBody] CreateRequestResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(resource.BuyerCompanyId)) return this.Forbidden();
        var result = await commandService.Handle(
            CreateRequestCommandFromResourceAssembler.ToCommandFromResource(resource), cancellationToken);
        return OrderingActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            created => CreatedAtAction(nameof(GetById), new { id = created.Id },
                RequestResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    [HttpPost("{id:int}/approve")]
    [Authorize("PROVIDER")]
    [SwaggerOperation(Summary = "Approve a fuel request and create an order", OperationId = "ApproveFuelRequest")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(OrderResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        var user = this.CurrentUser();
        var result = await orderCommandService.Handle(
            new ApproveFuelRequestCommand(id, user.Role, user.CompanyId),
            cancellationToken);
        return OrderingActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            order => Ok(OrderResourceFromEntityAssembler.ToResourceFromEntity(order)));
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update a fuel request", OperationId = "UpdateRequest")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(RequestResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRequestResource resource,
        CancellationToken cancellationToken)
    {
        var request = await queryService.Handle(new GetRequestByIdQuery(id), cancellationToken);
        if (request is null)
            return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status404NotFound,
                OrderingError.NotFound, errorLocalizer[nameof(OrderingError.NotFound)]);

        var user = this.CurrentUser();
        if (user.Role == "BUYER")
        {
            if (!this.OwnsBuyerCompany(request.BuyerCompanyId)) return this.Forbidden();
            if (request.Status != "PENDING")
                return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status400BadRequest,
                    OrderingError.ValidationError, "Only pending fuel requests can be modified by the buyer.");
            if (resource.BuyerCompanyId != request.BuyerCompanyId || resource.ProviderId != request.ProviderId)
                return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status400BadRequest,
                    OrderingError.ValidationError, "Reassigning company ownership is not allowed.");
        }
        else if (user.Role == "PROVIDER")
        {
            if (!this.OwnsProviderCompany(request.ProviderId)) return this.Forbidden();
            if (resource.Status != "REJECTED")
                return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status400BadRequest,
                    OrderingError.ValidationError, "Providers can only reject requests via PUT. Use POST /api/v1/fuel-requests/{id}/approve to approve.");
        }
        else
        {
            return this.Forbidden();
        }

        var result = await commandService.Handle(
            UpdateRequestCommandFromResourceAssembler.ToCommandFromResource(id, resource), cancellationToken);
        return OrderingActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            updated => Ok(RequestResourceFromEntityAssembler.ToResourceFromEntity(updated)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var request = await queryService.Handle(new GetRequestByIdQuery(id), cancellationToken);
        if (request is not null
            && !this.OwnsBuyerCompany(request.BuyerCompanyId)
            && !this.OwnsProviderCompany(request.ProviderId))
            return this.Forbidden();
        var result = await commandService.Handle(new DeleteRequestCommand(id), cancellationToken);
        return OrderingActionResultAssembler.ToActionResult(this, result, problemDetailsFactory, () => NoContent());
    }
}
