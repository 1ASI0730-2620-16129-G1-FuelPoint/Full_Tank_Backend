namespace TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Resources;

public record UpdatePaymentResource(
    int? OrderId,
    int? CompanyId,
    int? ProviderId,
    string Method,
    decimal Amount,
    string Status,
    string? MaskedCard,
    string? CardHolder,
    string? Reference);
