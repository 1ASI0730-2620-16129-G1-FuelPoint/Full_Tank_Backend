using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Transform;

public static class RefillHistoryResourceFromEntityAssembler
{
    public static RefillHistoryResource ToResourceFromEntity(RefillHistory entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new RefillHistoryResource(entity.Id, entity.EquipmentId, entity.CompanyId, entity.ProviderId,
            entity.FuelType, entity.Quantity, entity.RequestId, entity.CreatedAt, entity.UpdatedAt);
    }
}
