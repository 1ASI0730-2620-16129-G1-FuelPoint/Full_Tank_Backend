namespace TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Resources;

public record EquipmentResource(
    int Id,
    int? CompanyId,
    string Name,
    string Type,
    string RequiredFuelType,
    int Capacity,
    int CurrentLevel,
    string Unit,
    string Status,
    int? FavoriteProviderId,
    bool AutoRefill,
    int RefillThreshold,
    string Location,
    string? LastRefillDate,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
