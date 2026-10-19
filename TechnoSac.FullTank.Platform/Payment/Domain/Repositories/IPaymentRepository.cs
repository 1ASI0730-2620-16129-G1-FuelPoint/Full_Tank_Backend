using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using PaymentAggregate = TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates.Payment;

namespace TechnoSac.FullTank.Platform.Payment.Domain.Repositories;

public interface IPaymentRepository : IBaseRepository<PaymentAggregate>
{
    Task<IEnumerable<PaymentAggregate>> FindByCompanyIdAsync(int companyId, CancellationToken cancellationToken);
    Task<IEnumerable<PaymentAggregate>> FindByOrderIdAsync(int orderId, CancellationToken cancellationToken);
    Task<PaymentAggregate?> FindOneByOrderIdAsync(int orderId, CancellationToken cancellationToken);
}
