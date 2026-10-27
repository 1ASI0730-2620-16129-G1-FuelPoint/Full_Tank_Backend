using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using NotificationAggregate = TechnoSac.FullTank.Platform.Notification.Domain.Model.Aggregates.Notification;

namespace TechnoSac.FullTank.Platform.Notification.Domain.Repositories;

public interface INotificationRepository : IBaseRepository<NotificationAggregate>
{
    Task<IEnumerable<NotificationAggregate>> FindByBuyerCompanyIdAsync(int buyerCompanyId,
        CancellationToken cancellationToken);

    Task<IEnumerable<NotificationAggregate>> FindByProviderIdAsync(int providerId,
        CancellationToken cancellationToken);
}
