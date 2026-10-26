using TechnoSac.FullTank.Platform.Payment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Payment.Domain.Model;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Payment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PaymentAggregate = TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates.Payment;

namespace TechnoSac.FullTank.Platform.Payment.Application.Internal.CommandServices;

public class PaymentCommandService(
    IPaymentRepository repository,
    IOrderingContextFacade orderingContextFacade,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IPaymentCommandService
{
    public async Task<Result<PaymentAggregate>> Handle(CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        if (!await IsValid(command, cancellationToken))
            return Result<PaymentAggregate>.Failure(PaymentError.ValidationError,
                localizer[nameof(PaymentError.ValidationError)]);

        if (await repository.FindOneByOrderIdAsync(command.OrderId!.Value, cancellationToken) is not null)
            return Result<PaymentAggregate>.Failure(PaymentError.ValidationError,
                localizer[nameof(PaymentError.ValidationError)]);

        var payment = new PaymentAggregate(command);
        try
        {
            await repository.AddAsync(payment, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<PaymentAggregate>.Success(payment);
        }
        catch (DbUpdateException)
        {
            return Result<PaymentAggregate>.Failure(PaymentError.DatabaseError,
                localizer[nameof(PaymentError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<PaymentAggregate>.Failure(PaymentError.InternalServerError,
                localizer[nameof(PaymentError.InternalServerError)]);
        }
    }

    private async Task<bool> IsValid(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        if (command.OrderId is null or <= 0
            || command.CompanyId is null or <= 0
            || command.ProviderId is null or <= 0
            || command.Amount <= 0
            || !string.Equals(command.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase)
            || command.Method is not ("CARD" or "YAPE"))
            return false;

        var orderId = command.OrderId.Value;
        if (!string.Equals(await orderingContextFacade.FetchOrderStatus(orderId, cancellationToken),
                "PENDING_PAYMENT", StringComparison.OrdinalIgnoreCase))
            return false;

        var buyerCompanyId = await orderingContextFacade.FetchOrderBuyerCompanyId(orderId, cancellationToken);
        var providerId = await orderingContextFacade.FetchOrderProviderId(orderId, cancellationToken);
        var totalAmount = await orderingContextFacade.FetchOrderTotalAmount(orderId, cancellationToken);

        return buyerCompanyId == command.CompanyId
               && providerId == command.ProviderId
               && totalAmount == command.Amount;
    }

    public async Task<Result<PaymentAggregate>> Handle(UpdatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var payment = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (payment is null)
            return Result<PaymentAggregate>.Failure(PaymentError.NotFound, localizer[nameof(PaymentError.NotFound)]);

        try
        {
            payment.Update(command);
            repository.Update(payment);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<PaymentAggregate>.Success(payment);
        }
        catch (Exception)
        {
            return Result<PaymentAggregate>.Failure(PaymentError.InternalServerError,
                localizer[nameof(PaymentError.InternalServerError)]);
        }
    }
}
