namespace TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;

public record CheckoutPaymentCommand(
    CreatePaymentCommand Payment,
    CreateInvoiceCommand Invoice,
    int? ActorCompanyId = null);
