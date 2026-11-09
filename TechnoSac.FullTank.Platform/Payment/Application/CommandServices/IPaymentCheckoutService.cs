using TechnoSac.FullTank.Platform.Payment.Domain.Model;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Payment.Application.CommandServices;

public interface IPaymentCheckoutService
{
    Task<Result<PaymentCheckout>> Handle(CheckoutPaymentCommand command, CancellationToken cancellationToken);
}
