using TechnoSac.FullTank.Platform.Payment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Payment.Domain.Model;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Payment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Payment.Application.Internal.CommandServices;

public class InvoiceCommandService(
    IInvoiceRepository repository,
    IPaymentRepository paymentRepository,
    IOrderingContextFacade orderingContextFacade,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IInvoiceCommandService
{
    public async Task<Result<Invoice>> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        if (!await IsValid(command, cancellationToken))
            return Result<Invoice>.Failure(PaymentError.ValidationError,
                localizer[nameof(PaymentError.ValidationError)]);

        if (await repository.FindByPaymentIdAsync(command.PaymentId!.Value, cancellationToken) is not null
            || await repository.FindByOrderIdAsync(command.OrderId!.Value, cancellationToken) is not null)
            return Result<Invoice>.Failure(PaymentError.ValidationError,
                localizer[nameof(PaymentError.ValidationError)]);

        var invoice = new Invoice(command);
        try
        {
            await repository.AddAsync(invoice, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Invoice>.Success(invoice);
        }
        catch (DbUpdateException)
        {
            return Result<Invoice>.Failure(PaymentError.DatabaseError,
                localizer[nameof(PaymentError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<Invoice>.Failure(PaymentError.InternalServerError,
                localizer[nameof(PaymentError.InternalServerError)]);
        }
    }

    private async Task<bool> IsValid(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        if (command.PaymentId is null or <= 0
            || command.OrderId is null or <= 0
            || string.IsNullOrWhiteSpace(command.InvoiceNumber)
            || command.Quantity <= 0
            || command.UnitPrice < 0
            || command.Subtotal < 0
            || command.Igv < 0
            || command.Total <= 0
            || !string.Equals(command.Status, "PAID", StringComparison.OrdinalIgnoreCase))
            return false;

        var payment = await paymentRepository.FindByIdAsync(command.PaymentId.Value, cancellationToken);
        if (payment is null
            || payment.OrderId != command.OrderId
            || !string.Equals(payment.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase)
            || payment.Amount != command.Total)
            return false;

        var orderTotal = await orderingContextFacade.FetchOrderTotalAmount(command.OrderId.Value, cancellationToken);
        return orderTotal == command.Total;
    }

    public async Task<Result<Invoice>> Handle(UpdateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var invoice = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (invoice is null)
            return Result<Invoice>.Failure(PaymentError.NotFound, localizer[nameof(PaymentError.NotFound)]);

        try
        {
            invoice.Update(command);
            repository.Update(invoice);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Invoice>.Success(invoice);
        }
        catch (Exception)
        {
            return Result<Invoice>.Failure(PaymentError.InternalServerError,
                localizer[nameof(PaymentError.InternalServerError)]);
        }
    }
}
