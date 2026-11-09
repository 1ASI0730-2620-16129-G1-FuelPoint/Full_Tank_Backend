using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;

/// <summary>
///     Aggregate root representing a fuel delivery request made by a buyer company to a provider.
/// </summary>
/// <remarks>External references (BuyerCompanyId, ProviderId, EquipmentId) are kept as primitive IDs.</remarks>
public class Request : IAuditableEntity
{
    protected Request()
    {
        FuelType = string.Empty;
        ProductName = string.Empty;
        Unit = string.Empty;
        DeliveryAddress = string.Empty;
        DeliveryDate = string.Empty;
        Status = string.Empty;
        Source = string.Empty;
    }

    public Request(CreateRequestCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(command));
        if (command.UnitPrice < 0)
            throw new ArgumentException("UnitPrice cannot be negative.", nameof(command));

        BuyerCompanyId = command.BuyerCompanyId;
        ProviderId = command.ProviderId;
        EquipmentId = command.EquipmentId;
        FuelType = command.FuelType;
        ProductName = command.ProductName;
        Quantity = command.Quantity;
        Unit = command.Unit;
        UnitPrice = command.UnitPrice;
        DeliveryAddress = command.DeliveryAddress;
        DeliveryDate = command.DeliveryDate;
        Status = string.IsNullOrWhiteSpace(command.Status) ? "PENDING" : command.Status;
        Source = string.IsNullOrWhiteSpace(command.Source) ? "MANUAL" : command.Source;
        RejectionReasonCode = command.RejectionReasonCode;
        RejectionReasonNote = command.RejectionReasonNote;
    }

    public void Update(UpdateRequestCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(command));
        if (command.UnitPrice < 0)
            throw new ArgumentException("UnitPrice cannot be negative.", nameof(command));

        // Preserve BuyerCompanyId, ProviderId, EquipmentId - do not overwrite
        FuelType = command.FuelType;
        ProductName = command.ProductName;
        Quantity = command.Quantity;
        Unit = command.Unit;
        UnitPrice = command.UnitPrice;
        DeliveryAddress = command.DeliveryAddress;
        DeliveryDate = command.DeliveryDate;
        Status = command.Status;
        Source = command.Source;
        RejectionReasonCode = command.RejectionReasonCode;
        RejectionReasonNote = command.RejectionReasonNote;
    }

    public void Approve()
    {
        if (Status != "PENDING")
            throw new InvalidOperationException($"Cannot approve request in '{Status}' status. Request must be in 'PENDING' status.");
        Status = "APPROVED";
    }

    public void Reject(string reasonCode, string? reasonNote)
    {
        if (Status != "PENDING")
            throw new InvalidOperationException($"Cannot reject request in '{Status}' status. Request must be in 'PENDING' status.");
        Status = "REJECTED";
        RejectionReasonCode = reasonCode;
        RejectionReasonNote = reasonNote;
    }

    public void Cancel()
    {
        if (Status != "PENDING")
            throw new InvalidOperationException($"Cannot cancel request in '{Status}' status. Request must be in 'PENDING' status.");
        Status = "CANCELLED";
    }

    public int Id { get; private set; }
    public int? BuyerCompanyId { get; private set; }
    public int? ProviderId { get; private set; }
    public int? EquipmentId { get; private set; }
    public string FuelType { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public string Unit { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string DeliveryAddress { get; private set; }
    public string DeliveryDate { get; private set; }
    public string Status { get; private set; }
    public string Source { get; private set; }
    public string? RejectionReasonCode { get; private set; }
    public string? RejectionReasonNote { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
