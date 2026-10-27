using TechnoSac.FullTank.Platform.Notification.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Notification.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Notification.Domain.Repositories;
using TechnoSac.FullTank.Platform.Notification.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Notification.Application.Acl;

public class NotificationContextFacade(
    INotificationRepository repository,
    IUnitOfWork unitOfWork) : INotificationContextFacade
{
    public async Task<bool> CreateNotificationAsync(string recipientType, int? buyerCompanyId, int? providerId,
        string type, string title, string message, int? relatedId, string targetRoute,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = new Domain.Model.Aggregates.Notification(new CreateNotificationCommand(
                recipientType,
                buyerCompanyId,
                providerId,
                type,
                title,
                message,
                false,
                relatedId,
                targetRoute));

            await repository.AddAsync(notification, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
