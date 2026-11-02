using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>EF Core implementation of <see cref="IProviderCompanyRepository" />.</summary>
public class ProviderCompanyRepository(AppDbContext context)
    : BaseRepository<ProviderCompany>(context), IProviderCompanyRepository
{
    public async Task<bool> ExistsByRucAsync(string ruc, CancellationToken cancellationToken)
    {
        return await Context.Set<ProviderCompany>().AnyAsync(company => company.Ruc == ruc, cancellationToken);
    }
}
