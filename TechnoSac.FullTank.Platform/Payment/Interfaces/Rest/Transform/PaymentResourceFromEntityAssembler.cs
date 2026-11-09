using TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Resources;
using PaymentAggregate = TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates.Payment;

namespace TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Transform;

public static class PaymentResourceFromEntityAssembler
{
    public static PaymentResource ToResourceFromEntity(PaymentAggregate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new PaymentResource(entity.Id, entity.OrderId, entity.CompanyId, entity.ProviderId, entity.Method,
            entity.Amount, entity.Status, entity.MaskedCard, entity.CardHolder, entity.Reference, entity.CreatedAt,
            entity.UpdatedAt);
    }
}
