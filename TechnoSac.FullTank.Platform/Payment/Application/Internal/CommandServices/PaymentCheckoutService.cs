using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Notification.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Payment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Payment.Domain.Model;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Events;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Internal.EventBus;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Payment.Application.Internal.CommandServices;

public class PaymentCheckoutService(
    IPaymentCommandService paymentCommandService,
    IInvoiceCommandService invoiceCommandService,
    IOrderingContextFacade orderingContextFacade,
    IIamContextFacade iamContextFacade,
    INotificationContextFacade notificationContextFacade,
    IEventBus eventBus,
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    IStringLocalizer<ErrorMessages> localizer) : IPaymentCheckoutService
{
    public async Task<Result<PaymentCheckout>> Handle(CheckoutPaymentCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Strict Simulation Configuration Check
        // All current payment implementation is simulation. Without gateway configuration, reject all checkouts.
        var enableSimulation = configuration.GetValue<bool>("PaymentSettings:EnableSimulation");
        if (!enableSimulation)
        {
            return Result<PaymentCheckout>.Failure(PaymentError.ValidationError,
                "Live payment gateway is not configured. Simulated checkout is disabled in this environment.");
        }

        // 2. Validate Order Identifier
        if (command.Payment.OrderId is not int orderId || orderId <= 0)
            return Result<PaymentCheckout>.Failure(PaymentError.ValidationError,
                localizer[nameof(PaymentError.ValidationError)]);

        // 3. Begin SERIALIZABLE Transaction BEFORE Authoritative Reads and State Mutation
        await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            // 4. Validate Order Existence & Pending Payment Status
            if (!await orderingContextFacade.ExistsOrder(orderId, cancellationToken))
                return await Rollback(PaymentError.NotFound, localizer[nameof(PaymentError.NotFound)], cancellationToken);

            var orderStatus = await orderingContextFacade.FetchOrderStatus(orderId, cancellationToken);
            if (orderStatus != "PENDING_PAYMENT")
                return await Rollback(PaymentError.ValidationError, "Order is not in pending payment status.", cancellationToken);

            // 5. Authoritative Order Data & Actor Validation
            var orderBuyerCompanyId = await orderingContextFacade.FetchOrderBuyerCompanyId(orderId, cancellationToken);
            var orderProviderId = await orderingContextFacade.FetchOrderProviderId(orderId, cancellationToken);
            var orderTotalAmount = await orderingContextFacade.FetchOrderTotalAmount(orderId, cancellationToken);

            if (orderBuyerCompanyId is null || orderProviderId is null || orderTotalAmount is null or <= 0)
                return await Rollback(PaymentError.ValidationError, "Order data or total amount is invalid.", cancellationToken);

            if (!command.ActorCompanyId.HasValue || command.ActorCompanyId.Value <= 0 ||
                command.ActorCompanyId.Value != orderBuyerCompanyId.Value)
            {
                return await Rollback(PaymentError.ValidationError, "Payment company does not match order company.", cancellationToken);
            }

            // 6. Derive ALL line and tax metadata from server-side authoritative read models
            var orderLineInfo = await orderingContextFacade.FetchOrderLineInfo(orderId, cancellationToken);
            if (orderLineInfo is null)
                return await Rollback(PaymentError.ValidationError, "Authoritative order line information is unavailable.", cancellationToken);

            var buyerInfo = await iamContextFacade.FetchBuyerCompanyInfo(orderBuyerCompanyId.Value, cancellationToken);
            var providerInfo = await iamContextFacade.FetchProviderCompanyInfo(orderProviderId.Value, cancellationToken);

            if (string.IsNullOrWhiteSpace(buyerInfo.Name) || string.IsNullOrWhiteSpace(buyerInfo.Ruc) ||
                string.IsNullOrWhiteSpace(providerInfo.Name) || string.IsNullOrWhiteSpace(providerInfo.Ruc))
            {
                return await Rollback(PaymentError.ValidationError, "Authoritative company information is unavailable.", cancellationToken);
            }

            // 7. Server-Side Authoritative Financial Calculations (Peru 18% IGV)
            var total = orderLineInfo.Value.TotalAmount;
            var subtotal = Math.Round(total / 1.18m, 2, MidpointRounding.AwayFromZero);
            var igv = total - subtotal;

            // 8. Deterministic Unique Academic Receipt & Payment Reference tied to OrderId
            var invoiceNumber = $"SIM-INV-{orderId:D8}";
            var paymentReference = $"SIM-{orderId:D8}";

            // 9. Authoritative Commands (Ignoring untrusted client financial & party metadata)
            var paymentCommand = command.Payment with
            {
                OrderId = orderId,
                CompanyId = orderBuyerCompanyId.Value,
                ProviderId = orderProviderId.Value,
                Amount = total,
                Status = "COMPLETED",
                Reference = paymentReference
            };

            var invoiceCommand = command.Invoice with
            {
                OrderId = orderId,
                InvoiceNumber = invoiceNumber,
                ProviderRuc = providerInfo.Ruc,
                ProviderName = providerInfo.Name,
                BuyerRuc = buyerInfo.Ruc,
                BuyerName = buyerInfo.Name,
                FuelType = orderLineInfo.Value.FuelType,
                Quantity = orderLineInfo.Value.Quantity,
                Unit = orderLineInfo.Value.Unit,
                UnitPrice = orderLineInfo.Value.UnitPrice,
                Subtotal = subtotal,
                Igv = igv,
                Total = total,
                IssueDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Status = "PAID"
            };

            // 10. Execute Payment and Invoice within Transaction
            var paymentResult = await paymentCommandService.Handle(paymentCommand, cancellationToken);
            if (paymentResult.IsFailure)
                return await Rollback(paymentResult.Error!, paymentResult.Message, cancellationToken);

            var invoiceWithPayment = invoiceCommand with { PaymentId = paymentResult.Value!.Id };
            var invoiceResult = await invoiceCommandService.Handle(invoiceWithPayment, cancellationToken);
            if (invoiceResult.IsFailure)
                return await Rollback(invoiceResult.Error!, invoiceResult.Message, cancellationToken);

            if (!await orderingContextFacade.MarkOrderPaid(orderId, cancellationToken))
                return await Rollback(PaymentError.ValidationError,
                    localizer[nameof(PaymentError.ValidationError)], cancellationToken);

            // 11. Transaction-Guarded Notification & Domain Event (Kept INSIDE transaction)
            var notificationCreated = await notificationContextFacade.CreateNotificationAsync(
                "PROVIDER",
                orderBuyerCompanyId.Value,
                orderProviderId.Value,
                "PAYMENT_RECEIVED",
                "Payment Received",
                $"Payment of {total:C} received for Order #{orderId}. Receipt #{invoiceNumber} generated.",
                orderId,
                $"/ordering/orders/{orderId}",
                cancellationToken);

            if (!notificationCreated)
            {
                return await Rollback(PaymentError.InternalServerError,
                    localizer[nameof(PaymentError.InternalServerError)], cancellationToken);
            }

            await eventBus.PublishAsync(new PaymentCompletedEvent(
                paymentResult.Value!.Id,
                orderId,
                orderBuyerCompanyId.Value,
                orderProviderId.Value,
                total,
                invoiceNumber), cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<PaymentCheckout>.Success(new PaymentCheckout(paymentResult.Value, invoiceResult.Value!));
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<PaymentCheckout>.Failure(PaymentError.InternalServerError,
                localizer[nameof(PaymentError.InternalServerError)]);
        }
    }

    private async Task<Result<PaymentCheckout>> Rollback(Enum error, string message,
        CancellationToken cancellationToken)
    {
        await unitOfWork.RollbackTransactionAsync(cancellationToken);
        var paymentError = error is PaymentError value ? value : PaymentError.ValidationError;
        return Result<PaymentCheckout>.Failure(paymentError, message);
    }
}
