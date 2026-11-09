using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a buyer's fuel-consuming equipment (vehicle, machine, tank).</summary>
public class Equipment : IAuditableEntity
{
    protected Equipment()
    {
        Name = string.Empty;
        Type = string.Empty;
        RequiredFuelType = string.Empty;
        Unit = string.Empty;
        Status = string.Empty;
        Location = string.Empty;
    }

    public Equipment(CreateEquipmentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        CompanyId = command.CompanyId;
        Apply(command.Name, command.Type, command.RequiredFuelType, command.Capacity,
            command.CurrentLevel, command.Unit, command.Status, command.FavoriteProviderId, command.AutoRefill,
            command.RefillThreshold, command.Location, command.LastRefillDate);
    }

    public void Update(UpdateEquipmentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Apply(command.Name, command.Type, command.RequiredFuelType, command.Capacity,
            command.CurrentLevel, command.Unit, command.Status, command.FavoriteProviderId, command.AutoRefill,
            command.RefillThreshold, command.Location, command.LastRefillDate);
    }

    public void Refill(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Refill quantity must be greater than zero.", nameof(quantity));
        CurrentLevel = Math.Min(Capacity, CurrentLevel + quantity);
        LastRefillDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm");
        DeriveStatus();
    }

    public void DeriveStatus()
    {
        if (string.Equals(Status, "INACTIVE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Status, "OUT_OF_SERVICE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Status, "MAINTENANCE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Status, "DECOMMISSIONED", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if ((RefillThreshold > 0 && CurrentLevel <= RefillThreshold / 2) || CurrentLevel <= Capacity * 0.1)
        {
            Status = "critical";
        }
        else if ((RefillThreshold > 0 && CurrentLevel <= RefillThreshold) || CurrentLevel <= Capacity * 0.25)
        {
            Status = "low_fuel";
        }
        else
        {
            Status = "operational";
        }
    }

    public void SetStatus(string status)
    {
        if (!string.IsNullOrWhiteSpace(status))
            Status = status;
    }

    private void Apply(string name, string type, string requiredFuelType, int capacity,
        int currentLevel, string unit, string status, int? favoriteProviderId, bool autoRefill, int refillThreshold,
        string location, string? lastRefillDate)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));
        if (currentLevel < 0 || currentLevel > capacity)
            throw new ArgumentException("Current level must be non-negative and cannot exceed capacity.", nameof(currentLevel));
        if (refillThreshold < 0 || refillThreshold > capacity)
            throw new ArgumentException("Refill threshold must be non-negative and cannot exceed capacity.", nameof(refillThreshold));

        Name = name;
        Type = type;
        RequiredFuelType = requiredFuelType;
        Capacity = capacity;
        CurrentLevel = currentLevel;
        Unit = unit;
        Status = status;
        FavoriteProviderId = favoriteProviderId;
        AutoRefill = autoRefill;
        RefillThreshold = refillThreshold;
        Location = location;
        LastRefillDate = lastRefillDate;
    }

    public int Id { get; private set; }
    public int? CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string RequiredFuelType { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public int CurrentLevel { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public int? FavoriteProviderId { get; private set; }
    public bool AutoRefill { get; private set; }
    public int RefillThreshold { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public string? LastRefillDate { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
