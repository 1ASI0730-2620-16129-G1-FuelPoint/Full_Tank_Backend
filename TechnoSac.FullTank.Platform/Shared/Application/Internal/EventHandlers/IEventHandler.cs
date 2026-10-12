using TechnoSac.FullTank.Platform.Shared.Domain.Model.Events;
using Cortex.Mediator.Notifications;

namespace TechnoSac.FullTank.Platform.Shared.Application.Internal.EventHandlers;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
}
