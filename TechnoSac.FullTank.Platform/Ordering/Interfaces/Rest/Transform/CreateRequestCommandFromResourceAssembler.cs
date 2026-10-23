using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Transform;

public static class CreateRequestCommandFromResourceAssembler
{
    public static CreateRequestCommand ToCommandFromResource(CreateRequestResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateRequestCommand(resource.BuyerCompanyId, resource.ProviderId, resource.EquipmentId,
            resource.FuelType, resource.ProductName, resource.Quantity, resource.Unit, resource.UnitPrice,
            resource.DeliveryAddress, resource.DeliveryDate, resource.Status, resource.Source,
            resource.RejectionReasonCode, resource.RejectionReasonNote);
    }
}
