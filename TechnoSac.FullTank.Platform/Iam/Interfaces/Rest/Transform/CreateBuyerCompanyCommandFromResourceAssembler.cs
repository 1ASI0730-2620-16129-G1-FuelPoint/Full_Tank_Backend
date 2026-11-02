using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms a <see cref="CreateBuyerCompanyResource" /> into a <see cref="CreateBuyerCompanyCommand" />.</summary>
public static class CreateBuyerCompanyCommandFromResourceAssembler
{
    public static CreateBuyerCompanyCommand ToCommandFromResource(CreateBuyerCompanyResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateBuyerCompanyCommand(
            resource.Name,
            resource.Ruc,
            resource.Sector,
            resource.Address,
            resource.ContactEmail,
            resource.Phone);
    }
}
