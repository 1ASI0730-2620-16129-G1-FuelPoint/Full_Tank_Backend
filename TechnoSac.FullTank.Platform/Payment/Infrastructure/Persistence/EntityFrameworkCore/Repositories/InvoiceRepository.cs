using TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Payment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Payment.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class InvoiceRepository(AppDbContext context) : BaseRepository<Invoice>(context), IInvoiceRepository
{
    public async Task<Invoice?> FindByPaymentIdAsync(int paymentId, CancellationToken cancellationToken)
    {
        return await Context.Set<Invoice>()
            .FirstOrDefaultAsync(invoice => invoice.PaymentId == paymentId, cancellationToken);
    }

    public async Task<Invoice?> FindByOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        return await Context.Set<Invoice>()
            .FirstOrDefaultAsync(invoice => invoice.OrderId == orderId, cancellationToken);
    }
}
