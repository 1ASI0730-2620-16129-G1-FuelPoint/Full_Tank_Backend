using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms a <see cref="CreateProviderCompanyResource" /> into a <see cref="CreateProviderCompanyCommand" />.</summary>
public static class CreateProviderCompanyCommandFromResourceAssembler
{
    public static CreateProviderCompanyCommand ToCommandFromResource(CreateProviderCompanyResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateProviderCompanyCommand(
            resource.Name,
            resource.Ruc,
            resource.Address,
            resource.Phone,
            resource.Rating,
            resource.FuelTypesOffered,
            resource.Description);
    }
}
