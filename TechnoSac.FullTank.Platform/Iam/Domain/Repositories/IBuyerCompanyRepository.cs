using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Iam.Domain.Repositories;

/// <summary>Repository contract for the <see cref="BuyerCompany" /> aggregate.</summary>
public interface IBuyerCompanyRepository : IBaseRepository<BuyerCompany>
{
    /// <summary>Checks whether a buyer company already exists with the given RUC.</summary>
    Task<bool> ExistsByRucAsync(string ruc, CancellationToken cancellationToken);
}
