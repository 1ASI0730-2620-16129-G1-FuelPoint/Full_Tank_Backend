namespace TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Resources;

public record InvoiceResource(
    int Id,
    int? PaymentId,
    int? OrderId,
    string InvoiceNumber,
    string ProviderRuc,
    string ProviderName,
    string BuyerRuc,
    string BuyerName,
    string FuelType,
    int Quantity,
    string Unit,
    decimal UnitPrice,
    decimal Subtotal,
    decimal Igv,
    decimal Total,
    string? IssueDate,
    string Status,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
