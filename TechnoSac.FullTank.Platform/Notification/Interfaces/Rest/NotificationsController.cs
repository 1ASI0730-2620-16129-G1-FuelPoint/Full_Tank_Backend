using System.Net.Mime;
using TechnoSac.FullTank.Platform.Notification.Application.CommandServices;
using TechnoSac.FullTank.Platform.Notification.Application.QueryServices;
using TechnoSac.FullTank.Platform.Notification.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Notification.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Notification.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Notification.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace TechnoSac.FullTank.Platform.Notification.Interfaces.Rest;

[ApiController]
[Authorize("BUYER", "PROVIDER")]
[Route("api/v1/notifications")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Notification endpoints")]
public class NotificationsController(
    INotificationCommandService commandService,
    INotificationQueryService queryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get all notifications", OperationId = "GetAllNotifications")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<NotificationResource>))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await queryService.Handle(new GetAllNotificationsQuery(), cancellationToken);
        items = items.Where(CanAccess);
        return Ok(items.Select(NotificationResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a notification by id", OperationId = "GetNotificationById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(NotificationResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetNotificationByIdQuery(id), cancellationToken);
        if (item is not null && !CanAccess(item)) return this.Forbidden();
        return NotificationActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(NotificationResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpGet("buyer/{buyerCompanyId:int}")]
    [SwaggerOperation(Summary = "Get notifications by buyer company id", OperationId = "GetNotificationsByBuyer")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<NotificationResource>))]
    public async Task<IActionResult> GetByBuyer(int buyerCompanyId, CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(buyerCompanyId)) return this.Forbidden();
        var items = await queryService.Handle(new GetNotificationsByBuyerCompanyIdQuery(buyerCompanyId),
            cancellationToken);
        return Ok(items.Select(NotificationResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("provider/{providerId:int}")]
    [SwaggerOperation(Summary = "Get notifications by provider id", OperationId = "GetNotificationsByProvider")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<NotificationResource>))]
    public async Task<IActionResult> GetByProvider(int providerId, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var items = await queryService.Handle(new GetNotificationsByProviderIdQuery(providerId), cancellationToken);
        return Ok(items.Select(NotificationResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a notification", OperationId = "CreateNotification")]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(NotificationResource))]
    public async Task<IActionResult> Create([FromBody] CreateNotificationResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(
            CreateNotificationCommandFromResourceAssembler.ToCommandFromResource(resource), cancellationToken);
        return NotificationActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            created => CreatedAtAction(nameof(GetById), new { id = created.Id },
                NotificationResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update a notification", OperationId = "UpdateNotification")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(NotificationResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNotificationResource resource,
        CancellationToken cancellationToken)
    {
        var notification = await queryService.Handle(new GetNotificationByIdQuery(id), cancellationToken);
        if (notification is not null && (!CanAccess(notification) || !CanAccess(resource))) return this.Forbidden();

        var result = await commandService.Handle(
            UpdateNotificationCommandFromResourceAssembler.ToCommandFromResource(id, resource), cancellationToken);
        return NotificationActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            updated => Ok(NotificationResourceFromEntityAssembler.ToResourceFromEntity(updated)));
    }

    [HttpPost("{id:int}/read")]
    [SwaggerOperation(Summary = "Mark a notification as read", OperationId = "MarkNotificationAsRead")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(NotificationResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        var notification = await queryService.Handle(new GetNotificationByIdQuery(id), cancellationToken);
        if (notification is not null && !CanAccess(notification)) return this.Forbidden();

        var result = await commandService.Handle(new MarkNotificationAsReadCommand(id), cancellationToken);
        return NotificationActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            updated => Ok(NotificationResourceFromEntityAssembler.ToResourceFromEntity(updated)));
    }

    [HttpPost("buyer/{buyerCompanyId:int}/read-all")]
    [SwaggerOperation(Summary = "Mark all buyer notifications as read", OperationId = "MarkAllBuyerNotificationsAsRead")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "No content")]
    public async Task<IActionResult> MarkAllBuyerAsRead(int buyerCompanyId, CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(buyerCompanyId)) return this.Forbidden();
        var result = await commandService.Handle(new MarkAllBuyerNotificationsAsReadCommand(buyerCompanyId),
            cancellationToken);
        return NotificationActionResultAssembler.ToActionResult(this, result, problemDetailsFactory, NoContent);
    }

    [HttpPost("provider/{providerId:int}/read-all")]
    [SwaggerOperation(Summary = "Mark all provider notifications as read",
        OperationId = "MarkAllProviderNotificationsAsRead")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "No content")]
    public async Task<IActionResult> MarkAllProviderAsRead(int providerId, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var result = await commandService.Handle(new MarkAllProviderNotificationsAsReadCommand(providerId),
            cancellationToken);
        return NotificationActionResultAssembler.ToActionResult(this, result, problemDetailsFactory, NoContent);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var notification = await queryService.Handle(new GetNotificationByIdQuery(id), cancellationToken);
        if (notification is not null && !CanAccess(notification)) return this.Forbidden();

        var result = await commandService.Handle(new DeleteNotificationCommand(id), cancellationToken);
        return NotificationActionResultAssembler.ToActionResult(this, result, problemDetailsFactory, NoContent);
    }

    private bool CanAccess(Domain.Model.Aggregates.Notification notification)
    {
        return this.OwnsBuyerCompany(notification.BuyerCompanyId)
               || this.OwnsProviderCompany(notification.ProviderId);
    }

    private bool CanAccess(UpdateNotificationResource resource)
    {
        return this.OwnsBuyerCompany(resource.BuyerCompanyId)
               || this.OwnsProviderCompany(resource.ProviderId);
    }
}
