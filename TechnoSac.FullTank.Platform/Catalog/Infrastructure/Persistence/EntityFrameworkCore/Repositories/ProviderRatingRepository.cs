using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Catalog.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class ProviderRatingRepository(AppDbContext context)
    : BaseRepository<ProviderRating>(context), IProviderRatingRepository
{
    public async Task<IEnumerable<ProviderRating>> FindAsync(int? companyId, int? providerId,
        CancellationToken cancellationToken)
    {
        var query = Context.Set<ProviderRating>().AsQueryable();
        if (companyId.HasValue)
            query = query.Where(rating => rating.CompanyId == companyId.Value);
        if (providerId.HasValue)
            query = query.Where(rating => rating.ProviderId == providerId.Value);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ProviderRating?> FindByCompanyAndProviderAsync(int companyId, int providerId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<ProviderRating>().FirstOrDefaultAsync(
            rating => rating.CompanyId == companyId && rating.ProviderId == providerId, cancellationToken);
    }
}
