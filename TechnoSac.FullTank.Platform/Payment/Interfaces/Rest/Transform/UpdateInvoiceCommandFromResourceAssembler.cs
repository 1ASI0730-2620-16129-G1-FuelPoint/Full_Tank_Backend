using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Transform;

public static class UpdateInvoiceCommandFromResourceAssembler
{
    public static UpdateInvoiceCommand ToCommandFromResource(int id, UpdateInvoiceResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateInvoiceCommand(id, resource.PaymentId, resource.OrderId, resource.InvoiceNumber,
            resource.ProviderRuc, resource.ProviderName, resource.BuyerRuc, resource.BuyerName, resource.FuelType,
            resource.Quantity, resource.Unit, resource.UnitPrice, resource.Subtotal, resource.Igv, resource.Total,
            resource.IssueDate, resource.Status);
    }
}
