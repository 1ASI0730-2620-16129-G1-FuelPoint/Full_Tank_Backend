using TechnoSac.FullTank.Platform.Iam.Application.QueryServices;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Iam.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Iam.Application.Internal.QueryServices;

/// <summary>Handles buyer company queries.</summary>
public class BuyerCompanyQueryService(IBuyerCompanyRepository buyerCompanyRepository) : IBuyerCompanyQueryService
{
    public async Task<IEnumerable<BuyerCompany>> Handle(GetAllBuyerCompaniesQuery query,
        CancellationToken cancellationToken)
    {
        return await buyerCompanyRepository.ListAsync(cancellationToken);
    }

    public async Task<BuyerCompany?> Handle(GetBuyerCompanyByIdQuery query, CancellationToken cancellationToken)
    {
        return await buyerCompanyRepository.FindByIdAsync(query.Id, cancellationToken);
    }
}
