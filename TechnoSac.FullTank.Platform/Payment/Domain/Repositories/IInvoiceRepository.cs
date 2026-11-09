using TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Payment.Domain.Repositories;

public interface IInvoiceRepository : IBaseRepository<Invoice>
{
    Task<Invoice?> FindByPaymentIdAsync(int paymentId, CancellationToken cancellationToken);
    Task<Invoice?> FindByOrderIdAsync(int orderId, CancellationToken cancellationToken);
}
