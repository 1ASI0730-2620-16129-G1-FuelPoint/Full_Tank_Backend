using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms an <see cref="UpdateBuyerCompanyResource" /> into an <see cref="UpdateBuyerCompanyCommand" />.</summary>
public static class UpdateBuyerCompanyCommandFromResourceAssembler
{
    public static UpdateBuyerCompanyCommand ToCommandFromResource(int id, UpdateBuyerCompanyResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateBuyerCompanyCommand(
            id,
            resource.Name,
            resource.Ruc,
            resource.Sector,
            resource.Address,
            resource.ContactEmail,
            resource.Phone);
    }
}
