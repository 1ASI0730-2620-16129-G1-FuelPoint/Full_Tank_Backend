namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;

public record CreateRequestCommand(
    int? BuyerCompanyId,
    int? ProviderId,
    int? EquipmentId,
    string FuelType,
    string ProductName,
    int Quantity,
    string Unit,
    decimal UnitPrice,
    string DeliveryAddress,
    string DeliveryDate,
    string Status,
    string Source,
    string? RejectionReasonCode,
    string? RejectionReasonNote);
