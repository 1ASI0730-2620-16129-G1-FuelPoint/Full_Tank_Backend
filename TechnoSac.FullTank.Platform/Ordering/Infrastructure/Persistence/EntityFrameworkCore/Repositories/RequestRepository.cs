using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Ordering.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class RequestRepository(AppDbContext context) : BaseRepository<Request>(context), IRequestRepository
{
    public async Task<IEnumerable<Request>> FindByBuyerCompanyIdAsync(int buyerCompanyId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<Request>()
            .Where(request => request.BuyerCompanyId == buyerCompanyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken)
    {
        return await Context.Set<Request>()
            .Where(request => request.ProviderId == providerId)
            .ToListAsync(cancellationToken);
    }
}
