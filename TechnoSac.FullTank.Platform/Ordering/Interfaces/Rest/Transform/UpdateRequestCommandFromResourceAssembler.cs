using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Transform;

public static class UpdateRequestCommandFromResourceAssembler
{
    public static UpdateRequestCommand ToCommandFromResource(int id, UpdateRequestResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateRequestCommand(id, resource.BuyerCompanyId, resource.ProviderId, resource.EquipmentId,
            resource.FuelType, resource.ProductName, resource.Quantity, resource.Unit, resource.UnitPrice,
            resource.DeliveryAddress, resource.DeliveryDate, resource.Status, resource.Source,
            resource.RejectionReasonCode, resource.RejectionReasonNote);
    }
}
