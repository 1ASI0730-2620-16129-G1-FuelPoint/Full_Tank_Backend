using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>EF Core implementation of <see cref="IBuyerCompanyRepository" />.</summary>
public class BuyerCompanyRepository(AppDbContext context)
    : BaseRepository<BuyerCompany>(context), IBuyerCompanyRepository
{
    public async Task<bool> ExistsByRucAsync(string ruc, CancellationToken cancellationToken)
    {
        return await Context.Set<BuyerCompany>().AnyAsync(company => company.Ruc == ruc, cancellationToken);
    }
}
