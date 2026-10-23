using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Transform;

public static class RequestResourceFromEntityAssembler
{
    public static RequestResource ToResourceFromEntity(Request entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new RequestResource(entity.Id, entity.BuyerCompanyId, entity.ProviderId, entity.EquipmentId,
            entity.FuelType, entity.ProductName, entity.Quantity, entity.Unit, entity.UnitPrice, entity.DeliveryAddress,
            entity.DeliveryDate, entity.Status, entity.Source, entity.RejectionReasonCode, entity.RejectionReasonNote,
            entity.CreatedAt, entity.UpdatedAt);
    }
}
