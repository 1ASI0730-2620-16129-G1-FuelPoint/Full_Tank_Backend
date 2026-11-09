using TechnoSac.FullTank.Platform.Shared.Domain.Model.Events;

namespace TechnoSac.FullTank.Platform.Payment.Domain.Model.Events;

public record PaymentCompletedEvent(
    int PaymentId,
    int? OrderId,
    int? CompanyId,
    int? ProviderId,
    decimal Amount,
    string InvoiceNumber) : IEvent;
