using TechnoSac.FullTank.Platform.Notification.Application.QueryServices;
using TechnoSac.FullTank.Platform.Notification.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Notification.Domain.Repositories;
using NotificationAggregate = TechnoSac.FullTank.Platform.Notification.Domain.Model.Aggregates.Notification;

namespace TechnoSac.FullTank.Platform.Notification.Application.Internal.QueryServices;

public class NotificationQueryService(INotificationRepository repository) : INotificationQueryService
{
    public async Task<IEnumerable<NotificationAggregate>> Handle(GetAllNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }

    public async Task<NotificationAggregate?> Handle(GetNotificationByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<NotificationAggregate>> Handle(GetNotificationsByBuyerCompanyIdQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.FindByBuyerCompanyIdAsync(query.BuyerCompanyId, cancellationToken);
    }

    public async Task<IEnumerable<NotificationAggregate>> Handle(GetNotificationsByProviderIdQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.FindByProviderIdAsync(query.ProviderId, cancellationToken);
    }
}
