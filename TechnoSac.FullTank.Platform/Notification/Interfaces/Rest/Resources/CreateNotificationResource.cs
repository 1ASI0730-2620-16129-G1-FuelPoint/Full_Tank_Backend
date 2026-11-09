namespace TechnoSac.FullTank.Platform.Notification.Interfaces.Rest.Resources;

public record CreateNotificationResource(
    string RecipientType,
    int? BuyerCompanyId,
    int? ProviderId,
    string Type,
    string Title,
    string Message,
    bool IsRead,
    int? RelatedId,
    string TargetRoute);
