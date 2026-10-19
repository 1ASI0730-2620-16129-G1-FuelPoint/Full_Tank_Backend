using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using PaymentAggregate = TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates.Payment;

namespace TechnoSac.FullTank.Platform.Payment.Application.CommandServices;

public interface IPaymentCommandService
{
    Task<Result<PaymentAggregate>> Handle(CreatePaymentCommand command, CancellationToken cancellationToken);
    Task<Result<PaymentAggregate>> Handle(UpdatePaymentCommand command, CancellationToken cancellationToken);
}
