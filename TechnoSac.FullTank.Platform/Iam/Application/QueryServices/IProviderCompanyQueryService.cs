using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Iam.Application.QueryServices;

/// <summary>Query service contract for provider companies.</summary>
public interface IProviderCompanyQueryService
{
    Task<IEnumerable<ProviderCompany>> Handle(GetAllProviderCompaniesQuery query, CancellationToken cancellationToken);
    Task<ProviderCompany?> Handle(GetProviderCompanyByIdQuery query, CancellationToken cancellationToken);
}
