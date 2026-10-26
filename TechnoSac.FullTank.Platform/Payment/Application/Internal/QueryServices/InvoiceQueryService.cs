using TechnoSac.FullTank.Platform.Payment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Payment.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Payment.Application.Internal.QueryServices;

public class InvoiceQueryService(IInvoiceRepository repository) : IInvoiceQueryService
{
    public async Task<IEnumerable<Invoice>> Handle(GetAllInvoicesQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }

    public async Task<Invoice?> Handle(GetInvoiceByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<Invoice?> Handle(GetInvoiceByPaymentIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByPaymentIdAsync(query.PaymentId, cancellationToken);
    }

    public async Task<Invoice?> Handle(GetInvoiceByOrderIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByOrderIdAsync(query.OrderId, cancellationToken);
    }
}
