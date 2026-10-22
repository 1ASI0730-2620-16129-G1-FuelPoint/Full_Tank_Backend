namespace TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Resources;

public record CreateRefillHistoryResource(
    int EquipmentId,
    int? CompanyId,
    int? ProviderId,
    string FuelType,
    int Quantity,
    int? RequestId);
