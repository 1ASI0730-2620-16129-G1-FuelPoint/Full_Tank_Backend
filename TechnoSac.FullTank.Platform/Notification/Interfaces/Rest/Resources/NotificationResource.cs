namespace TechnoSac.FullTank.Platform.Notification.Interfaces.Rest.Resources;

public record NotificationResource(
    int Id,
    string RecipientType,
    int? BuyerCompanyId,
    int? ProviderId,
    string Type,
    string Title,
    string Message,
    bool IsRead,
    int? RelatedId,
    string TargetRoute,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
