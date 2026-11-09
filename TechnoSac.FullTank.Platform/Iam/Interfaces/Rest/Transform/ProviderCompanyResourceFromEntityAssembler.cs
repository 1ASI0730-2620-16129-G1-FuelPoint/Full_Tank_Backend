using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms a <see cref="ProviderCompany" /> aggregate into a <see cref="ProviderCompanyResource" />.</summary>
public static class ProviderCompanyResourceFromEntityAssembler
{
    public static ProviderCompanyResource ToResourceFromEntity(ProviderCompany company, string? registrationToken = null)
    {
        ArgumentNullException.ThrowIfNull(company);
        return new ProviderCompanyResource(
            company.Id,
            company.Name,
            company.Ruc,
            company.Address,
            company.Phone,
            company.Rating,
            company.FuelTypesAsList(),
            company.Description,
            company.CreatedAt,
            company.UpdatedAt,
            registrationToken);
    }
}
