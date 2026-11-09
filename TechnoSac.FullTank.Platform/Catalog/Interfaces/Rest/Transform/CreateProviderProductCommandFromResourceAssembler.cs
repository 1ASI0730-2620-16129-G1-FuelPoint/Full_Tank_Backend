using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Transform;

public static class CreateProviderProductCommandFromResourceAssembler
{
    public static CreateProviderProductCommand ToCommandFromResource(CreateProviderProductResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateProviderProductCommand(resource.ProviderId, resource.FuelType, resource.Name,
            resource.Description, resource.PricePerLiter, resource.Unit, resource.Available);
    }
}
