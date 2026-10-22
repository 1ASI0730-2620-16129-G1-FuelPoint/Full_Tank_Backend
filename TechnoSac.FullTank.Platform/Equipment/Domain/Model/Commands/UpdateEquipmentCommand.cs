namespace TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;

public record UpdateEquipmentCommand(
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
    string? LastRefillDate);
