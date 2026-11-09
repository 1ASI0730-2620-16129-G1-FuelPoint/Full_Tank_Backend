namespace TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;

public record CreateRefillHistoryCommand(
    int EquipmentId,
    int? CompanyId,
    int? ProviderId,
    string FuelType,
    int Quantity,
    int? RequestId);
