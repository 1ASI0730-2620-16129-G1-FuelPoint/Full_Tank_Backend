using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a simulated fuel payment.</summary>
public class Payment : IAuditableEntity
{
    protected Payment()
    {
        Method = string.Empty;
        Status = string.Empty;
    }

    public Payment(CreatePaymentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        OrderId = command.OrderId;
        CompanyId = command.CompanyId;
        ProviderId = command.ProviderId;
        Method = command.Method;
        Amount = command.Amount;
        Status = string.IsNullOrWhiteSpace(command.Status) ? "PENDING" : command.Status;
        MaskedCard = command.MaskedCard;
        CardHolder = command.CardHolder;
        Reference = command.Reference;
    }

    public void Update(UpdatePaymentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        OrderId = command.OrderId;
        CompanyId = command.CompanyId;
        ProviderId = command.ProviderId;
        Method = command.Method;
        Amount = command.Amount;
        Status = command.Status;
        MaskedCard = command.MaskedCard;
        CardHolder = command.CardHolder;
        Reference = command.Reference;
    }

    public int Id { get; private set; }
    public int? OrderId { get; private set; }
    public int? CompanyId { get; private set; }
    public int? ProviderId { get; private set; }
    public string Method { get; private set; }
    public decimal Amount { get; private set; }
    public string Status { get; private set; }
    public string? MaskedCard { get; private set; }
    public string? CardHolder { get; private set; }
    public string? Reference { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
