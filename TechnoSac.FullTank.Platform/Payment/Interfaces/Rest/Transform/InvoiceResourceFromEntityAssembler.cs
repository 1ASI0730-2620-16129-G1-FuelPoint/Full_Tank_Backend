using TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Transform;

public static class InvoiceResourceFromEntityAssembler
{
    public static InvoiceResource ToResourceFromEntity(Invoice entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new InvoiceResource(entity.Id, entity.PaymentId, entity.OrderId, entity.InvoiceNumber,
            entity.ProviderRuc, entity.ProviderName, entity.BuyerRuc, entity.BuyerName, entity.FuelType,
            entity.Quantity, entity.Unit, entity.UnitPrice, entity.Subtotal, entity.Igv, entity.Total,
            entity.IssueDate, entity.Status, entity.CreatedAt, entity.UpdatedAt);
    }
}
