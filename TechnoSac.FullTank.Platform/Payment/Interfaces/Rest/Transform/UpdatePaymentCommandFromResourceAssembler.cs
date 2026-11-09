using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Transform;

public static class UpdatePaymentCommandFromResourceAssembler
{
    public static UpdatePaymentCommand ToCommandFromResource(int id, UpdatePaymentResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdatePaymentCommand(id, resource.OrderId, resource.CompanyId, resource.ProviderId, resource.Method,
            resource.Amount, resource.Status, resource.MaskedCard, resource.CardHolder, resource.Reference);
    }
}
