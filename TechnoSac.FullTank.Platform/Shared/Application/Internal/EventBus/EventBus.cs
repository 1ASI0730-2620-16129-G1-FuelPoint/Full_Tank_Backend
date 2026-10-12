using Cortex.Mediator.Notifications;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Events;
using Microsoft.Extensions.DependencyInjection;

namespace TechnoSac.FullTank.Platform.Shared.Application.Internal.EventBus;

public class EventBus(IServiceProvider serviceProvider) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IEvent
    {
        var handlers = serviceProvider.GetServices<INotificationHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.Handle(@event, cancellationToken);
        }
    }
}
