using TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Payment.Application.QueryServices;

public interface IInvoiceQueryService
{
    Task<IEnumerable<Invoice>> Handle(GetAllInvoicesQuery query, CancellationToken cancellationToken);
    Task<Invoice?> Handle(GetInvoiceByIdQuery query, CancellationToken cancellationToken);
    Task<Invoice?> Handle(GetInvoiceByPaymentIdQuery query, CancellationToken cancellationToken);
    Task<Invoice?> Handle(GetInvoiceByOrderIdQuery query, CancellationToken cancellationToken);
}
