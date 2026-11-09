using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a fuel product offered by a provider in the catalog.</summary>
public class ProviderProduct : IAuditableEntity
{
    protected ProviderProduct()
    {
        FuelType = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        Unit = string.Empty;
    }

    public ProviderProduct(CreateProviderProductCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.PricePerLiter < 0)
            throw new ArgumentException("Price per liter cannot be negative.", nameof(command));
        ProviderId = command.ProviderId;
        FuelType = command.FuelType;
        Name = command.Name;
        Description = command.Description;
        PricePerLiter = command.PricePerLiter;
        Unit = command.Unit;
        Available = command.Available;
    }

    public void Update(UpdateProviderProductCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.PricePerLiter < 0)
            throw new ArgumentException("Price per liter cannot be negative.", nameof(command));
        FuelType = command.FuelType;
        Name = command.Name;
        Description = command.Description;
        PricePerLiter = command.PricePerLiter;
        Unit = command.Unit;
        Available = command.Available;
    }

    public int Id { get; private set; }
    public int? ProviderId { get; private set; }
    public string FuelType { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal PricePerLiter { get; private set; }
    public string Unit { get; private set; }
    public bool Available { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
