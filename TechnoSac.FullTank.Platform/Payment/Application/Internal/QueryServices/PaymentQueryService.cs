using TechnoSac.FullTank.Platform.Payment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Payment.Domain.Repositories;
using PaymentAggregate = TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates.Payment;

namespace TechnoSac.FullTank.Platform.Payment.Application.Internal.QueryServices;

public class PaymentQueryService(IPaymentRepository repository) : IPaymentQueryService
{
    public async Task<IEnumerable<PaymentAggregate>> Handle(GetAllPaymentsQuery query,
        CancellationToken cancellationToken)
    {
        return query.CompanyId is null
            ? await repository.ListAsync(cancellationToken)
            : await repository.FindByCompanyIdAsync(query.CompanyId.Value, cancellationToken);
    }

    public async Task<PaymentAggregate?> Handle(GetPaymentByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<PaymentAggregate>> Handle(GetPaymentsByOrderIdQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.FindByOrderIdAsync(query.OrderId, cancellationToken);
    }
}
