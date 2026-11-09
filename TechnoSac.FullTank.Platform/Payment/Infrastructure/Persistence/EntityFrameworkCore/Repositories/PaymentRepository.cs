using TechnoSac.FullTank.Platform.Payment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using PaymentAggregate = TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates.Payment;

namespace TechnoSac.FullTank.Platform.Payment.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class PaymentRepository(AppDbContext context)
    : BaseRepository<PaymentAggregate>(context), IPaymentRepository
{
    public async Task<IEnumerable<PaymentAggregate>> FindByCompanyIdAsync(int companyId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<PaymentAggregate>()
            .Where(payment => payment.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentAggregate>> FindByOrderIdAsync(int orderId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<PaymentAggregate>()
            .Where(payment => payment.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentAggregate?> FindOneByOrderIdAsync(int orderId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<PaymentAggregate>()
            .FirstOrDefaultAsync(payment => payment.OrderId == orderId, cancellationToken);
    }
}
