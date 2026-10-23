namespace TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Resources;

public record CreateRequestResource(
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
    string Status = "",
    string Source = "",
    string? RejectionReasonCode = null,
    string? RejectionReasonNote = null);
