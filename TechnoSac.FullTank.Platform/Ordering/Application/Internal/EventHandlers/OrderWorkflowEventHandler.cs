using Microsoft.Extensions.Logging;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Events;
using TechnoSac.FullTank.Platform.Shared.Application.Internal.EventHandlers;

namespace TechnoSac.FullTank.Platform.Ordering.Application.Internal.EventHandlers;

public class OrderWorkflowEventHandler(ILogger<OrderWorkflowEventHandler> logger)
    : IEventHandler<RequestApprovedEvent>,
      IEventHandler<OrderDispatchedEvent>,
      IEventHandler<OrderReceivedEvent>,
      IEventHandler<OrderCancelledEvent>
{
    public Task Handle(RequestApprovedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("DomainEvent: Fuel request #{RequestId} approved into Order #{OrderId} ({Quantity} {FuelType}).",
            notification.RequestId, notification.OrderId, notification.Quantity, notification.FuelType);
        return Task.CompletedTask;
    }

    public Task Handle(OrderDispatchedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("DomainEvent: Order #{OrderId} dispatched with Driver #{DriverId} and Vehicle #{VehicleId}.",
            notification.OrderId, notification.DriverId, notification.VehicleId);
        return Task.CompletedTask;
    }

    public Task Handle(OrderReceivedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("DomainEvent: Order #{OrderId} received by Buyer #{BuyerCompanyId}. Pending payment.",
            notification.OrderId, notification.BuyerCompanyId);
        return Task.CompletedTask;
    }

    public Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("DomainEvent: Order #{OrderId} cancelled. Reason: {Reason}.",
            notification.OrderId, notification.Reason);
        return Task.CompletedTask;
    }
}
