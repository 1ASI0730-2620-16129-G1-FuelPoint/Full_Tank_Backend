namespace TechnoSac.FullTank.Platform.Notification.Domain.Model;

/// <summary>Domain/application errors for the Notification bounded context.</summary>
public enum NotificationError
{
    None,
    NotFound,
    ValidationError,
    OperationCancelled,
    DatabaseError,
    InternalServerError
}
