using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model;
using System.Net.Mime;
using TechnoSac.FullTank.Platform.Fulfillment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Fulfillment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest;

[ApiController]
[Authorize("BUYER", "PROVIDER")]
[Route("api/v1/deliveries")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Delivery endpoints")]
public class DeliveriesController(
    IDeliveryCommandService commandService,
    IDeliveryQueryService queryService,
    IOrderingContextFacade orderingContextFacade,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get deliveries (optionally by providerId)", OperationId = "GetAllDeliveries")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<DeliveryResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? providerId, CancellationToken cancellationToken)
    {
        if (this.CurrentUser().Role == "PROVIDER")
            providerId = this.CurrentUser().CompanyId;

        var items = await queryService.Handle(new GetAllDeliveriesQuery(providerId), cancellationToken);
        items = await FilterAccessible(items, cancellationToken);
        return Ok(items.Select(DeliveryResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a delivery by id", OperationId = "GetDeliveryById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(DeliveryResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetDeliveryByIdQuery(id), cancellationToken);
        if (item is not null && !await CanAccess(item, cancellationToken)) return this.Forbidden();
        return FulfillmentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(DeliveryResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpGet("provider/{providerId:int}")]
    [SwaggerOperation(Summary = "Get deliveries by provider id", OperationId = "GetDeliveriesByProvider")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<DeliveryResource>))]
    public async Task<IActionResult> GetByProvider(int providerId, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var items = await queryService.Handle(new GetAllDeliveriesQuery(providerId), cancellationToken);
        return Ok(items.Select(DeliveryResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("order/{orderId:int}")]
    [SwaggerOperation(Summary = "Get deliveries by order id", OperationId = "GetDeliveriesByOrder")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<DeliveryResource>))]
    public async Task<IActionResult> GetByOrder(int orderId, CancellationToken cancellationToken)
    {
        if (!await CanAccessOrder(orderId, cancellationToken)) return this.Forbidden();
        var items = await queryService.Handle(new GetDeliveriesByOrderIdQuery(orderId), cancellationToken);
        return Ok(items.Select(DeliveryResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a delivery", OperationId = "CreateDelivery")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Direct delivery creation disabled")]
    public Task<IActionResult> Create([FromBody] CreateDeliveryResource resource,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<IActionResult>(problemDetailsFactory.CreateProblemDetails(
            this,
            StatusCodes.Status400BadRequest,
            FulfillmentError.ValidationError,
            "Direct delivery creation is disabled. Use order workflow dispatch transition: POST /api/v1/orders/{id}/dispatch."));
    }

    [HttpPost("{id:int}/complete")]
    [SwaggerOperation(Summary = "Complete a delivery", OperationId = "CompleteDelivery")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Direct delivery completion disabled")]
    public Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        return Task.FromResult<IActionResult>(problemDetailsFactory.CreateProblemDetails(
            this,
            StatusCodes.Status400BadRequest,
            FulfillmentError.ValidationError,
            "Direct delivery completion is disabled. Use order workflow receive transition: POST /api/v1/orders/{id}/receive."));
    }

    private async Task<IEnumerable<Domain.Model.Aggregates.Delivery>> FilterAccessible(
        IEnumerable<Domain.Model.Aggregates.Delivery> deliveries, CancellationToken cancellationToken)
    {
        var accessible = new List<Domain.Model.Aggregates.Delivery>();
        foreach (var delivery in deliveries)
            if (await CanAccess(delivery, cancellationToken))
                accessible.Add(delivery);
        return accessible;
    }

    private async Task<bool> CanAccess(Domain.Model.Aggregates.Delivery delivery,
        CancellationToken cancellationToken)
    {
        return this.OwnsProviderCompany(delivery.ProviderId)
               || delivery.OrderId is not null
               && await CanAccessOrder(delivery.OrderId.Value, cancellationToken);
    }

    private async Task<bool> CanAccessOrder(int orderId, CancellationToken cancellationToken)
    {
        var buyerCompanyId = await orderingContextFacade.FetchOrderBuyerCompanyId(orderId, cancellationToken);
        var providerId = await orderingContextFacade.FetchOrderProviderId(orderId, cancellationToken);
        return this.OwnsBuyerCompany(buyerCompanyId) || this.OwnsProviderCompany(providerId);
    }
}
