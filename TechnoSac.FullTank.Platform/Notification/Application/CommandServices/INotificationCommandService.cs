using TechnoSac.FullTank.Platform.Notification.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using NotificationAggregate = TechnoSac.FullTank.Platform.Notification.Domain.Model.Aggregates.Notification;

namespace TechnoSac.FullTank.Platform.Notification.Application.CommandServices;

public interface INotificationCommandService
{
    Task<Result<NotificationAggregate>> Handle(CreateNotificationCommand command, CancellationToken cancellationToken);
    Task<Result<NotificationAggregate>> Handle(UpdateNotificationCommand command, CancellationToken cancellationToken);
    Task<Result<NotificationAggregate>> Handle(MarkNotificationAsReadCommand command,
        CancellationToken cancellationToken);
    Task<Result> Handle(MarkAllBuyerNotificationsAsReadCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(MarkAllProviderNotificationsAsReadCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteNotificationCommand command, CancellationToken cancellationToken);
}
