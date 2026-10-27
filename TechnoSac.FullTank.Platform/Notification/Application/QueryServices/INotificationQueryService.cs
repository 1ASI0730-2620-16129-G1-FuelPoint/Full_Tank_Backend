using TechnoSac.FullTank.Platform.Notification.Domain.Model.Queries;
using NotificationAggregate = TechnoSac.FullTank.Platform.Notification.Domain.Model.Aggregates.Notification;

namespace TechnoSac.FullTank.Platform.Notification.Application.QueryServices;

public interface INotificationQueryService
{
    Task<IEnumerable<NotificationAggregate>> Handle(GetAllNotificationsQuery query, CancellationToken cancellationToken);
    Task<NotificationAggregate?> Handle(GetNotificationByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<NotificationAggregate>> Handle(GetNotificationsByBuyerCompanyIdQuery query,
        CancellationToken cancellationToken);
    Task<IEnumerable<NotificationAggregate>> Handle(GetNotificationsByProviderIdQuery query,
        CancellationToken cancellationToken);
}
