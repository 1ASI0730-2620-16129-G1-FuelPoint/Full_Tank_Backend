using TechnoSac.FullTank.Platform.Notification.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Notification.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Notification.Interfaces.Rest.Transform;

public static class CreateNotificationCommandFromResourceAssembler
{
    public static CreateNotificationCommand ToCommandFromResource(CreateNotificationResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateNotificationCommand(resource.RecipientType, resource.BuyerCompanyId, resource.ProviderId,
            resource.Type, resource.Title, resource.Message, resource.IsRead, resource.RelatedId,
            resource.TargetRoute);
    }
}
