using TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Resources;
using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;

namespace TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Transform;

public static class EquipmentResourceFromEntityAssembler
{
    public static EquipmentResource ToResourceFromEntity(EquipmentAggregate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new EquipmentResource(entity.Id, entity.CompanyId, entity.Name, entity.Type, entity.RequiredFuelType,
            entity.Capacity, entity.CurrentLevel, entity.Unit, entity.Status, entity.FavoriteProviderId,
            entity.AutoRefill, entity.RefillThreshold, entity.Location, entity.LastRefillDate, entity.CreatedAt,
            entity.UpdatedAt);
    }
}
