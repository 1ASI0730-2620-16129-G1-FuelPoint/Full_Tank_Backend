using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Transform;

public static class VehicleResourceFromEntityAssembler
{
    public static VehicleResource ToResourceFromEntity(Vehicle entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new VehicleResource(entity.Id, entity.Plate, entity.Brand, entity.Model, entity.Capacity, entity.Unit,
            entity.Status, entity.ProviderId, entity.CreatedAt, entity.UpdatedAt);
    }
}
