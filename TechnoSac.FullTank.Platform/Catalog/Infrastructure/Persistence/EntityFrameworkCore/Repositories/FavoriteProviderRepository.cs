using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Catalog.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class FavoriteProviderRepository(AppDbContext context)
    : BaseRepository<FavoriteProvider>(context), IFavoriteProviderRepository
{
    public async Task<IEnumerable<FavoriteProvider>> FindByCompanyIdAsync(int companyId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<FavoriteProvider>()
            .Where(favorite => favorite.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<FavoriteProvider?> FindByCompanyAndProviderAsync(int companyId, int providerId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<FavoriteProvider>()
            .FirstOrDefaultAsync(
                favorite => favorite.CompanyId == companyId && favorite.ProviderId == providerId,
                cancellationToken);
    }
}
