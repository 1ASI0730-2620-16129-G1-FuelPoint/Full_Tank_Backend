namespace TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;

public record CreatePaymentCommand(
    int? OrderId,
    int? CompanyId,
    int? ProviderId,
    string Method,
    decimal Amount,
    string Status,
    string? MaskedCard,
    string? CardHolder,
    string? Reference);
