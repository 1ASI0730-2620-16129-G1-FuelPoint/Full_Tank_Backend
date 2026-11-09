using TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Payment.Application.CommandServices;

public interface IInvoiceCommandService
{
    Task<Result<Invoice>> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken);
    Task<Result<Invoice>> Handle(UpdateInvoiceCommand command, CancellationToken cancellationToken);
}
