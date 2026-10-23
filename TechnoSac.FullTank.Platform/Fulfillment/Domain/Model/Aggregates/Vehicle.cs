using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a delivery vehicle (tanker).</summary>
public class Vehicle : IAuditableEntity
{
    protected Vehicle()
    {
        Plate = string.Empty;
        Brand = string.Empty;
        Model = string.Empty;
        Unit = string.Empty;
        Status = string.Empty;
    }

    public Vehicle(CreateVehicleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Capacity <= 0)
            throw new ArgumentException("Vehicle capacity must be greater than zero.", nameof(command));
        Plate = command.Plate;
        Brand = command.Brand;
        Model = command.Model;
        Capacity = command.Capacity;
        Unit = command.Unit;
        Status = command.Status;
        ProviderId = command.ProviderId;
    }

    public void Update(UpdateVehicleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Capacity <= 0)
            throw new ArgumentException("Vehicle capacity must be greater than zero.", nameof(command));
        Plate = command.Plate;
        Brand = command.Brand;
        Model = command.Model;
        Capacity = command.Capacity;
        Unit = command.Unit;
        Status = command.Status;
    }

    public void SetStatus(string status)
    {
        if (!string.IsNullOrWhiteSpace(status))
            Status = status;
    }

    public int Id { get; private set; }
    public string Plate { get; private set; }
    public string Brand { get; private set; }
    public string Model { get; private set; }
    public int Capacity { get; private set; }
    public string Unit { get; private set; }
    public string Status { get; private set; }
    public int? ProviderId { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
