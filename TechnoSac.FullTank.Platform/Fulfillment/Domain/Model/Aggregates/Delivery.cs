using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a delivery (dispatch of an order via a driver and vehicle).</summary>
public class Delivery : IAuditableEntity
{
    protected Delivery()
    {
        Status = string.Empty;
        OriginLocation = string.Empty;
        DestinationLocation = string.Empty;
    }

    public Delivery(CreateDeliveryCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        OrderId = command.OrderId;
        ProviderId = command.ProviderId;
        Apply(command.DriverId, command.VehicleId, command.Status,
            command.OriginLocation, command.DestinationLocation, command.DispatchedAt, command.DeliveredAt,
            command.Notes);
    }

    public void Update(UpdateDeliveryCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Apply(command.DriverId, command.VehicleId, command.Status,
            command.OriginLocation, command.DestinationLocation, command.DispatchedAt, command.DeliveredAt,
            command.Notes);
    }

    /// <summary>Marks the delivery as completed (delivered), stamping the delivery time.</summary>
    public void Complete(string? deliveredAt = null)
    {
        Status = "delivered";
        DeliveredAt = deliveredAt ?? DateTimeOffset.UtcNow.ToString("O");
    }

    public void Cancel()
    {
        Status = "cancelled";
    }

    private void Apply(int? driverId, int? vehicleId, string status,
        string originLocation, string destinationLocation, string? dispatchedAt, string? deliveredAt, string? notes)
    {
        DriverId = driverId;
        VehicleId = vehicleId;
        Status = status;
        OriginLocation = originLocation;
        DestinationLocation = destinationLocation;
        DispatchedAt = dispatchedAt;
        DeliveredAt = deliveredAt;
        Notes = notes;
    }

    public int Id { get; private set; }
    public int? OrderId { get; private set; }
    public int? ProviderId { get; private set; }
    public int? DriverId { get; private set; }
    public int? VehicleId { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string OriginLocation { get; private set; } = string.Empty;
    public string DestinationLocation { get; private set; } = string.Empty;
    public string? DispatchedAt { get; private set; }
    public string? DeliveredAt { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
