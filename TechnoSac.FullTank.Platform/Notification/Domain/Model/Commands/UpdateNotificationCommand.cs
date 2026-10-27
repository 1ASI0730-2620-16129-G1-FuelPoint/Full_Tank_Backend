namespace TechnoSac.FullTank.Platform.Notification.Domain.Model.Commands;

public record UpdateNotificationCommand(
    int Id,
    string RecipientType,
    int? BuyerCompanyId,
    int? ProviderId,
    string Type,
    string Title,
    string Message,
    bool IsRead,
    int? RelatedId,
    string TargetRoute);
