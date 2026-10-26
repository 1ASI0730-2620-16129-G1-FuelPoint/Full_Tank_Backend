using TechnoSac.FullTank.Platform.Payment.Domain.Model.Queries;
using PaymentAggregate = TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates.Payment;

namespace TechnoSac.FullTank.Platform.Payment.Application.QueryServices;

public interface IPaymentQueryService
{
    Task<IEnumerable<PaymentAggregate>> Handle(GetAllPaymentsQuery query, CancellationToken cancellationToken);
    Task<PaymentAggregate?> Handle(GetPaymentByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<PaymentAggregate>> Handle(GetPaymentsByOrderIdQuery query, CancellationToken cancellationToken);
}
