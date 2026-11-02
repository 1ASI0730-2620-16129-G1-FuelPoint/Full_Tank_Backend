using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Iam.Application.CommandServices;

/// <summary>Command service contract for buyer companies.</summary>
public interface IBuyerCompanyCommandService
{
    Task<Result<BuyerCompany>> Handle(CreateBuyerCompanyCommand command, CancellationToken cancellationToken);
    Task<Result<BuyerCompany>> Handle(UpdateBuyerCompanyCommand command, CancellationToken cancellationToken);
}
