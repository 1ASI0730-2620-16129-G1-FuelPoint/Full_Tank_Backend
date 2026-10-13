namespace TechnoSac.FullTank.Platform.Notification.Interfaces.Acl;

public interface INotificationContextFacade
{
    Task<bool> CreateNotificationAsync(string recipientType, int? buyerCompanyId, int? providerId, string type,
        string title, string message, int? relatedId, string targetRoute, CancellationToken cancellationToken);
}
