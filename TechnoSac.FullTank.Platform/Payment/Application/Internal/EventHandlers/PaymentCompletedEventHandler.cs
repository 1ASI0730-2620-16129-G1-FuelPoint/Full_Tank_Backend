using Microsoft.Extensions.Logging;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Events;
using TechnoSac.FullTank.Platform.Shared.Application.Internal.EventHandlers;

namespace TechnoSac.FullTank.Platform.Payment.Application.Internal.EventHandlers;

public class PaymentCompletedEventHandler(ILogger<PaymentCompletedEventHandler> logger)
    : IEventHandler<PaymentCompletedEvent>
{
    public Task Handle(PaymentCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "DomainEvent: Payment #{PaymentId} completed for Order #{OrderId}. Amount: {Amount:C}. Invoice: {InvoiceNumber}.",
            notification.PaymentId, notification.OrderId, notification.Amount, notification.InvoiceNumber);
        return Task.CompletedTask;
    }
}
