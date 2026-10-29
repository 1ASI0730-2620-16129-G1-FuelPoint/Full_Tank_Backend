using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Iam.Domain.Repositories;

/// <summary>Repository contract for the <see cref="ProviderCompany" /> aggregate.</summary>
public interface IProviderCompanyRepository : IBaseRepository<ProviderCompany>
{
    /// <summary>Checks whether a provider company already exists with the given RUC.</summary>
    Task<bool> ExistsByRucAsync(string ruc, CancellationToken cancellationToken);
}
