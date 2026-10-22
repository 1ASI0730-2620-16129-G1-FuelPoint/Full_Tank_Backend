using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Transform;

public static class CreateEquipmentCommandFromResourceAssembler
{
    public static CreateEquipmentCommand ToCommandFromResource(CreateEquipmentResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateEquipmentCommand(resource.CompanyId, resource.Name, resource.Type, resource.RequiredFuelType,
            resource.Capacity, resource.CurrentLevel, resource.Unit, resource.Status, resource.FavoriteProviderId,
            resource.AutoRefill, resource.RefillThreshold, resource.Location, resource.LastRefillDate);
    }
}
