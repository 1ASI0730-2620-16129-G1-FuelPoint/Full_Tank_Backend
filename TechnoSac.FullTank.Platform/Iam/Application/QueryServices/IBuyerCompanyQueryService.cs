using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Iam.Application.QueryServices;

/// <summary>Query service contract for buyer companies.</summary>
public interface IBuyerCompanyQueryService
{
    Task<IEnumerable<BuyerCompany>> Handle(GetAllBuyerCompaniesQuery query, CancellationToken cancellationToken);
    Task<BuyerCompany?> Handle(GetBuyerCompanyByIdQuery query, CancellationToken cancellationToken);
}
