using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms an <see cref="UpdateProviderCompanyResource" /> into an <see cref="UpdateProviderCompanyCommand" />.</summary>
public static class UpdateProviderCompanyCommandFromResourceAssembler
{
    public static UpdateProviderCompanyCommand ToCommandFromResource(int id, UpdateProviderCompanyResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateProviderCompanyCommand(
            id,
            resource.Name,
            resource.Ruc,
            resource.Address,
            resource.Phone,
            resource.Rating,
            resource.FuelTypesOffered,
            resource.Description);
    }
}
