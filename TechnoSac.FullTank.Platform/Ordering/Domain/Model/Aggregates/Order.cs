using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;

/// <summary>
///     Aggregate root representing a confirmed fuel order, created when a provider accepts a request.
/// </summary>
/// <remarks>External references (RequestId, BuyerCompanyId, ProviderId, EquipmentId, DriverId, VehicleId)
///     are kept as primitive IDs.</remarks>
public class Order : IAuditableEntity
{
    protected Order()
    {
        FuelType = string.Empty;
        Unit = string.Empty;
        DeliveryAddress = string.Empty;
        Status = string.Empty;
        PaymentStatus = string.Empty;
    }

    public Order(CreateOrderCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Apply(command.RequestId, command.BuyerCompanyId, command.ProviderId, command.EquipmentId, command.FuelType,
            command.Quantity, command.Unit, command.UnitPrice, command.TotalAmount, command.DeliveryAddress,
            string.IsNullOrWhiteSpace(command.Status) ? "ACCEPTED" : command.Status,
            string.IsNullOrWhiteSpace(command.PaymentStatus) ? "PENDING" : command.PaymentStatus,
            command.DriverId, command.VehicleId, command.EstimatedDeliveryDate, command.DispatchedAt,
            command.DeliveredAt, command.PaidAt, command.ClosedAt, command.CancelledAt, command.CancelReason);
    }

    public void Update(UpdateOrderCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Apply(command.FuelType,
            command.Quantity, command.Unit, command.UnitPrice, command.TotalAmount, command.DeliveryAddress,
            command.Status, command.PaymentStatus, command.DriverId, command.VehicleId, command.EstimatedDeliveryDate,
            command.DispatchedAt, command.DeliveredAt, command.PaidAt, command.ClosedAt, command.CancelledAt,
            command.CancelReason);
    }

    public void Dispatch(int driverId, int vehicleId, string? dispatchedAt = null)
    {
        if (Status != "ACCEPTED")
            throw new InvalidOperationException($"Cannot dispatch order in '{Status}' status. Order must be in 'ACCEPTED' status.");
        DriverId = driverId;
        VehicleId = vehicleId;
        Status = "DISPATCHED";
        DispatchedAt = dispatchedAt ?? DateTimeOffset.UtcNow.ToString("O");
    }

    public void Receive(string? deliveredAt = null)
    {
        if (Status != "DISPATCHED")
            throw new InvalidOperationException($"Cannot receive order in '{Status}' status. Order must be in 'DISPATCHED' status.");
        Status = "PENDING_PAYMENT";
        DeliveredAt = deliveredAt ?? DateTimeOffset.UtcNow.ToString("O");
    }

    public void Cancel(string reason, string? cancelledAt = null)
    {
        if (Status is "PAID" or "CLOSED" or "CANCELLED")
            throw new InvalidOperationException($"Cannot cancel order in '{Status}' status.");
        Status = "CANCELLED";
        CancelReason = reason;
        CancelledAt = cancelledAt ?? DateTimeOffset.UtcNow.ToString("O");
    }

    public void MarkPaid(string? paidAt = null)
    {
        PaymentStatus = "PAID";
        Status = "PAID";
        PaidAt = paidAt ?? DateTimeOffset.UtcNow.ToString("O");
    }

    public void Close(string? closedAt = null)
    {
        Status = "CLOSED";
        ClosedAt = closedAt ?? DateTimeOffset.UtcNow.ToString("O");
    }

    private void Apply(int? requestId, int? buyerCompanyId, int? providerId, int? equipmentId, string fuelType,
        int quantity, string unit, decimal unitPrice, decimal totalAmount, string deliveryAddress, string status,
        string paymentStatus, int? driverId, int? vehicleId, string? estimatedDeliveryDate, string? dispatchedAt,
        string? deliveredAt, string? paidAt, string? closedAt, string? cancelledAt, string? cancelReason)
    {
        RequestId = requestId;
        BuyerCompanyId = buyerCompanyId;
        ProviderId = providerId;
        EquipmentId = equipmentId;
        Apply(fuelType, quantity, unit, unitPrice, totalAmount, deliveryAddress, status, paymentStatus,
            driverId, vehicleId, estimatedDeliveryDate, dispatchedAt, deliveredAt, paidAt, closedAt, cancelledAt, cancelReason);
    }

    private void Apply(string fuelType,
        int quantity, string unit, decimal unitPrice, decimal totalAmount, string deliveryAddress, string status,
        string paymentStatus, int? driverId, int? vehicleId, string? estimatedDeliveryDate, string? dispatchedAt,
        string? deliveredAt, string? paidAt, string? closedAt, string? cancelledAt, string? cancelReason)
    {
        FuelType = fuelType;
        Quantity = quantity;
        Unit = unit;
        UnitPrice = unitPrice;
        TotalAmount = totalAmount;
        DeliveryAddress = deliveryAddress;
        Status = status;
        PaymentStatus = paymentStatus;
        DriverId = driverId;
        VehicleId = vehicleId;
        EstimatedDeliveryDate = estimatedDeliveryDate;
        DispatchedAt = dispatchedAt;
        DeliveredAt = deliveredAt;
        PaidAt = paidAt;
        ClosedAt = closedAt;
        CancelledAt = cancelledAt;
        CancelReason = cancelReason;
    }

    public int Id { get; private set; }
    public int? RequestId { get; private set; }
    public int? BuyerCompanyId { get; private set; }
    public int? ProviderId { get; private set; }
    public int? EquipmentId { get; private set; }
    public string FuelType { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string DeliveryAddress { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string PaymentStatus { get; private set; } = string.Empty;
    public int? DriverId { get; private set; }
    public int? VehicleId { get; private set; }
    public string? EstimatedDeliveryDate { get; private set; }
    public string? DispatchedAt { get; private set; }
    public string? DeliveredAt { get; private set; }
    public string? PaidAt { get; private set; }
    public string? ClosedAt { get; private set; }
    public string? CancelledAt { get; private set; }
    public string? CancelReason { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
