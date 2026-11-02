using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms a <see cref="BuyerCompany" /> aggregate into a <see cref="BuyerCompanyResource" />.</summary>
public static class BuyerCompanyResourceFromEntityAssembler
{
    public static BuyerCompanyResource ToResourceFromEntity(BuyerCompany company, string? registrationToken = null)
    {
        ArgumentNullException.ThrowIfNull(company);
        return new BuyerCompanyResource(
            company.Id,
            company.Name,
            company.Ruc,
            company.Sector,
            company.Address,
            company.ContactEmail,
            company.Phone,
            company.CreatedAt,
            company.UpdatedAt,
            registrationToken);
    }
}
