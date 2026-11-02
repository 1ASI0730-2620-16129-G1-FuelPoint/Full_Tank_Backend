using TechnoSac.FullTank.Platform.Iam.Application.QueryServices;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Iam.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Iam.Application.Internal.QueryServices;

/// <summary>Handles provider company queries.</summary>
public class ProviderCompanyQueryService(IProviderCompanyRepository providerCompanyRepository)
    : IProviderCompanyQueryService
{
    public async Task<IEnumerable<ProviderCompany>> Handle(GetAllProviderCompaniesQuery query,
        CancellationToken cancellationToken)
    {
        return await providerCompanyRepository.ListAsync(cancellationToken);
    }

    public async Task<ProviderCompany?> Handle(GetProviderCompanyByIdQuery query, CancellationToken cancellationToken)
    {
        return await providerCompanyRepository.FindByIdAsync(query.Id, cancellationToken);
    }
}
