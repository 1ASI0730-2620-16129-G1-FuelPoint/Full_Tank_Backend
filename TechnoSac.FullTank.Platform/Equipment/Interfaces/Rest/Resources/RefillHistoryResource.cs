namespace TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Resources;

public record RefillHistoryResource(
    int Id,
    int EquipmentId,
    int? CompanyId,
    int? ProviderId,
    string FuelType,
    int Quantity,
    int? RequestId,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
