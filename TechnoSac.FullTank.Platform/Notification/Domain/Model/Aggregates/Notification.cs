using TechnoSac.FullTank.Platform.Notification.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Notification.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a notification addressed to a buyer company or a provider.</summary>
public class Notification : IAuditableEntity
{
    protected Notification()
    {
        RecipientType = string.Empty;
        Type = string.Empty;
        Title = string.Empty;
        Message = string.Empty;
        TargetRoute = string.Empty;
    }

    public Notification(CreateNotificationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Apply(command.RecipientType, command.BuyerCompanyId, command.ProviderId, command.Type, command.Title,
            command.Message, command.IsRead, command.RelatedId, command.TargetRoute);
    }

    public void Update(UpdateNotificationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Apply(command.RecipientType, command.BuyerCompanyId, command.ProviderId, command.Type, command.Title,
            command.Message, command.IsRead, command.RelatedId, command.TargetRoute);
    }

    /// <summary>Marks this notification as read.</summary>
    public void MarkAsRead()
    {
        IsRead = true;
    }

    private void Apply(string recipientType, int? buyerCompanyId, int? providerId, string type, string title,
        string message, bool isRead, int? relatedId, string targetRoute)
    {
        RecipientType = recipientType;
        BuyerCompanyId = buyerCompanyId;
        ProviderId = providerId;
        Type = type;
        Title = title;
        Message = message;
        IsRead = isRead;
        RelatedId = relatedId;
        TargetRoute = targetRoute;
    }

    public int Id { get; private set; }
    public string RecipientType { get; private set; } = string.Empty;
    public int? BuyerCompanyId { get; private set; }
    public int? ProviderId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public int? RelatedId { get; private set; }
    public string TargetRoute { get; private set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
