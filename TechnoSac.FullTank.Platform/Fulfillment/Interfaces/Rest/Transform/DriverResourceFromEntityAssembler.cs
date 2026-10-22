using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Transform;

public static class DriverResourceFromEntityAssembler
{
    public static DriverResource ToResourceFromEntity(Driver entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new DriverResource(entity.Id, entity.Name, entity.LicenseNumber, entity.Phone, entity.Email,
            entity.Status, entity.ProviderId, entity.CreatedAt, entity.UpdatedAt);
    }
}
