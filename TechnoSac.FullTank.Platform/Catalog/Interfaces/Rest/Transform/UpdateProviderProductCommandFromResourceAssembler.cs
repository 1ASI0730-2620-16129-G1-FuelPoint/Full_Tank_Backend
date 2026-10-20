using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Transform;

public static class UpdateProviderProductCommandFromResourceAssembler
{
    public static UpdateProviderProductCommand ToCommandFromResource(int id, UpdateProviderProductResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateProviderProductCommand(id, resource.ProviderId, resource.FuelType, resource.Name,
            resource.Description, resource.PricePerLiter, resource.Unit, resource.Available);
    }
}
