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
[Route("api/v1/orders")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Order endpoints")]
[Authorize("BUYER", "PROVIDER")]
public class OrdersController(
    IOrderCommandService commandService,
    IOrderQueryService queryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get orders (optionally by buyerCompanyId or providerId)", OperationId = "GetAllOrders")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<OrderResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? buyerCompanyId, [FromQuery] int? providerId,
        CancellationToken cancellationToken)
    {
        var user = this.CurrentUser();
        buyerCompanyId = user.Role == "BUYER" ? user.CompanyId : null;
        providerId = user.Role == "PROVIDER" ? user.CompanyId : null;
        var items = await queryService.Handle(new GetAllOrdersQuery(buyerCompanyId, providerId), cancellationToken);
        return Ok(items.Select(OrderResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get an order by id", OperationId = "GetOrderById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(OrderResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetOrderByIdQuery(id), cancellationToken);
        if (item is not null
            && !this.OwnsBuyerCompany(item.BuyerCompanyId)
            && !this.OwnsProviderCompany(item.ProviderId))
            return this.Forbidden();
        return OrderingActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer, problemDetailsFactory,
            found => Ok(OrderResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [Authorize("PROVIDER")]
    [SwaggerOperation(Summary = "Create an order (disabled)", OperationId = "CreateOrder")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request")]
    public IActionResult Create([FromBody] CreateOrderResource resource, CancellationToken cancellationToken)
    {
        return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status400BadRequest,
            OrderingError.ValidationError,
            "Direct order creation is disabled. Use POST /api/v1/fuel-requests/{id}/approve.");
    }

    [HttpPost("{id:int}/dispatch")]
    [Authorize("PROVIDER")]
    [SwaggerOperation(Summary = "Dispatch an accepted order with driver and vehicle", OperationId = "DispatchOrder")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(OrderResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Dispatch(int id, [FromBody] DispatchOrderResource resource,
        CancellationToken cancellationToken)
    {
        var user = this.CurrentUser();
        var result = await commandService.Handle(
            new DispatchOrderCommand(id, resource.DriverId, resource.VehicleId, user.Role, user.CompanyId),
            cancellationToken);
        return OrderingActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            order => Ok(OrderResourceFromEntityAssembler.ToResourceFromEntity(order)));
    }

    [HttpPost("{id:int}/receive")]
    [Authorize("BUYER")]
    [SwaggerOperation(Summary = "Confirm receipt of a dispatched order", OperationId = "ReceiveOrder")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(OrderResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Receive(int id, CancellationToken cancellationToken)
    {
        var user = this.CurrentUser();
        var result = await commandService.Handle(
            new ReceiveOrderCommand(id, user.Role, user.CompanyId),
            cancellationToken);
        return OrderingActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            order => Ok(OrderResourceFromEntityAssembler.ToResourceFromEntity(order)));
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize("PROVIDER")]
    [SwaggerOperation(Summary = "Cancel an order", OperationId = "CancelOrder")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(OrderResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelOrderResource? resource,
        CancellationToken cancellationToken)
    {
        var user = this.CurrentUser();
        var result = await commandService.Handle(
            new CancelOrderCommand(id, resource?.Reason ?? "Cancelled by provider", user.Role, user.CompanyId),
            cancellationToken);
        return OrderingActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            order => Ok(OrderResourceFromEntityAssembler.ToResourceFromEntity(order)));
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update an order (disabled)", OperationId = "UpdateOrder")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request")]
    public IActionResult Update(int id, [FromBody] UpdateOrderResource resource,
        CancellationToken cancellationToken)
    {
        return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status400BadRequest,
            OrderingError.ValidationError,
            "Orders cannot be updated via generic PUT. Use dedicated workflow transitions: dispatch, receive, cancel.");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var order = await queryService.Handle(new GetOrderByIdQuery(id), cancellationToken);
        if (order is not null && !this.OwnsProviderCompany(order.ProviderId)) return this.Forbidden();
        var result = await commandService.Handle(new DeleteOrderCommand(id), cancellationToken);
        return OrderingActionResultAssembler.ToActionResult(this, result, problemDetailsFactory, () => NoContent());
    }
}
