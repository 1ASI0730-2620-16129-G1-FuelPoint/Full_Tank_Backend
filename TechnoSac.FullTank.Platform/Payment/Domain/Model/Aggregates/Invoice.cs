using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a fuel purchase invoice.</summary>
public class Invoice : IAuditableEntity
{
    protected Invoice()
    {
        InvoiceNumber = string.Empty;
        ProviderRuc = string.Empty;
        ProviderName = string.Empty;
        BuyerRuc = string.Empty;
        BuyerName = string.Empty;
        FuelType = string.Empty;
        Unit = string.Empty;
        Status = string.Empty;
    }

    public Invoice(CreateInvoiceCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Apply(command.PaymentId, command.OrderId, command.InvoiceNumber, command.ProviderRuc, command.ProviderName,
            command.BuyerRuc, command.BuyerName, command.FuelType, command.Quantity, command.Unit, command.UnitPrice,
            command.Subtotal, command.Igv, command.Total, command.IssueDate,
            string.IsNullOrWhiteSpace(command.Status) ? "PAID" : command.Status);
    }

    public void Update(UpdateInvoiceCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Apply(command.PaymentId, command.OrderId, command.InvoiceNumber, command.ProviderRuc, command.ProviderName,
            command.BuyerRuc, command.BuyerName, command.FuelType, command.Quantity, command.Unit, command.UnitPrice,
            command.Subtotal, command.Igv, command.Total, command.IssueDate, command.Status);
    }

    private void Apply(int? paymentId, int? orderId, string invoiceNumber, string providerRuc, string providerName,
        string buyerRuc, string buyerName, string fuelType, int quantity, string unit, decimal unitPrice,
        decimal subtotal, decimal igv, decimal total, string? issueDate, string status)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        InvoiceNumber = invoiceNumber;
        ProviderRuc = providerRuc;
        ProviderName = providerName;
        BuyerRuc = buyerRuc;
        BuyerName = buyerName;
        FuelType = fuelType;
        Quantity = quantity;
        Unit = unit;
        UnitPrice = unitPrice;
        Subtotal = subtotal;
        Igv = igv;
        Total = total;
        IssueDate = issueDate;
        Status = status;
    }

    public int Id { get; private set; }
    public int? PaymentId { get; private set; }
    public int? OrderId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public string ProviderRuc { get; private set; } = string.Empty;
    public string ProviderName { get; private set; } = string.Empty;
    public string BuyerRuc { get; private set; } = string.Empty;
    public string BuyerName { get; private set; } = string.Empty;
    public string FuelType { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Igv { get; private set; }
    public decimal Total { get; private set; }
    public string? IssueDate { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
