using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Transform;

public static class CreateRefillHistoryCommandFromResourceAssembler
{
    public static CreateRefillHistoryCommand ToCommandFromResource(CreateRefillHistoryResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateRefillHistoryCommand(resource.EquipmentId, resource.CompanyId, resource.ProviderId,
            resource.FuelType, resource.Quantity, resource.RequestId);
    }
}
