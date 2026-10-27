using TechnoSac.FullTank.Platform.Notification.Interfaces.Rest.Resources;
using NotificationAggregate = TechnoSac.FullTank.Platform.Notification.Domain.Model.Aggregates.Notification;

namespace TechnoSac.FullTank.Platform.Notification.Interfaces.Rest.Transform;

public static class NotificationResourceFromEntityAssembler
{
    public static NotificationResource ToResourceFromEntity(NotificationAggregate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new NotificationResource(entity.Id, entity.RecipientType, entity.BuyerCompanyId, entity.ProviderId,
            entity.Type, entity.Title, entity.Message, entity.IsRead, entity.RelatedId, entity.TargetRoute,
            entity.CreatedAt, entity.UpdatedAt);
    }
}
