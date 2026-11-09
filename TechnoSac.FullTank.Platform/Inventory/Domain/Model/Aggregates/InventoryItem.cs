using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a provider inventory item (fuel storage / stock).</summary>
public class InventoryItem : IAuditableEntity
{
    protected InventoryItem()
    {
        Name = string.Empty;
        Type = string.Empty;
        Description = string.Empty;
        Unit = string.Empty;
        Status = string.Empty;
    }

    public InventoryItem(CreateInventoryItemCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        ProviderId = command.ProviderId;
        Apply(command.Name, command.Type, command.Description, command.PricePerLiter,
            command.Stock, command.Reserved, command.Capacity, command.LowStockThreshold, command.Unit, command.Status);
    }

    public void Update(UpdateInventoryItemCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Apply(command.Name, command.Type, command.Description, command.PricePerLiter,
            command.Stock, command.Reserved, command.Capacity, command.LowStockThreshold, command.Unit, command.Status);
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Reservation quantity must be greater than zero.", nameof(quantity));
        if (Stock - Reserved < quantity)
            throw new InvalidOperationException($"Insufficient inventory stock. Available: {Stock - Reserved}, Requested: {quantity}.");
        Reserved += quantity;
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Release quantity must be greater than zero.", nameof(quantity));
        Reserved = Math.Max(0, Reserved - quantity);
    }

    public void FulfillReservation(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Fulfill quantity must be greater than zero.", nameof(quantity));
        if (Stock < quantity)
            throw new InvalidOperationException($"Stock ({Stock}) is less than fulfillment quantity ({quantity}).");
        Stock -= quantity;
        Reserved = Math.Max(0, Reserved - quantity);
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Restock quantity must be greater than zero.", nameof(quantity));
        if (Stock + quantity > Capacity)
            throw new InvalidOperationException($"Restock exceeds capacity ({Capacity}). Current stock: {Stock}, Added: {quantity}.");
        Stock += quantity;
    }

    private void Apply(string name, string type, string description, decimal pricePerLiter,
        int stock, int reserved, int capacity, int lowStockThreshold, string unit, string status)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));
        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative.", nameof(stock));
        if (reserved < 0)
            throw new ArgumentException("Reserved cannot be negative.", nameof(reserved));
        if (stock > capacity)
            throw new ArgumentException("Stock cannot exceed capacity.", nameof(stock));
        if (reserved > stock)
            throw new ArgumentException("Reserved cannot exceed stock.", nameof(reserved));
        if (pricePerLiter < 0)
            throw new ArgumentException("Price per liter cannot be negative.", nameof(pricePerLiter));

        Name = name;
        Type = type;
        Description = description;
        PricePerLiter = pricePerLiter;
        Stock = stock;
        Reserved = reserved;
        Capacity = capacity;
        LowStockThreshold = lowStockThreshold;
        Unit = unit;
        Status = status;
    }

    public int Id { get; private set; }
    public int? ProviderId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal PricePerLiter { get; private set; }
    public int Stock { get; private set; }
    public int Reserved { get; private set; }
    public int Capacity { get; private set; }
    public int LowStockThreshold { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
