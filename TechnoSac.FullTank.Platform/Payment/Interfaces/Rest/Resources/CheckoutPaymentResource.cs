namespace TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Resources;

public record CheckoutPaymentResource(
    CreatePaymentResource Payment,
    CreateInvoiceResource Invoice);

public record PaymentCheckoutResource(
    PaymentResource Payment,
    InvoiceResource Invoice);
