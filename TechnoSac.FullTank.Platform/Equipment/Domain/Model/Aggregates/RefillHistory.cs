using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a fuel refill event for a buyer equipment.</summary>
public class RefillHistory : IAuditableEntity
{
    protected RefillHistory()
    {
        FuelType = string.Empty;
    }

    public RefillHistory(CreateRefillHistoryCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        EquipmentId = command.EquipmentId;
        CompanyId = command.CompanyId;
        ProviderId = command.ProviderId;
        FuelType = command.FuelType;
        Quantity = command.Quantity;
        RequestId = command.RequestId;
    }

    public int Id { get; private set; }
    public int EquipmentId { get; private set; }
    public int? CompanyId { get; private set; }
    public int? ProviderId { get; private set; }
    public string FuelType { get; private set; }
    public int Quantity { get; private set; }
    public int? RequestId { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
