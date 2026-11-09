using TechnoSac.FullTank.Platform.Equipment.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Notification.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Ordering.Application.CommandServices;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Events;
using TechnoSac.FullTank.Platform.Ordering.Domain.Repositories;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Internal.EventBus;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Ordering.Application.Internal.CommandServices;

public class OrderCommandService(
    IOrderRepository repository,
    IRequestRepository requestRepository,
    IIamContextFacade iamContextFacade,
    IFulfillmentContextFacade fulfillmentContextFacade,
    IInventoryContextFacade inventoryContextFacade,
    IEquipmentContextFacade equipmentContextFacade,
    INotificationContextFacade notificationContextFacade,
    IEventBus eventBus,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IOrderCommandService
{
    public Task<Result<Order>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<Order>.Failure(OrderingError.ValidationError,
            "Direct order creation is disabled. Use POST /api/v1/fuel-requests/{id}/approve."));
    }

    public Task<Result<Order>> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<Order>.Failure(OrderingError.ValidationError,
            "Orders cannot be updated via generic PUT. Use dedicated workflow transitions: dispatch, receive, cancel."));
    }

    public async Task<Result> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (order is null)
            return Result.Failure(OrderingError.NotFound, localizer[nameof(OrderingError.NotFound)]);
        if (order.Status is not ("CANCELLED" or "CLOSED"))
            return Result.Failure(OrderingError.OperationCancelled,
                "Active orders cannot be deleted. Only CANCELLED or CLOSED orders may be removed.");

        repository.Remove(order);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<Order>> Handle(ApproveFuelRequestCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            var request = await requestRepository.FindByIdAsync(command.RequestId, cancellationToken);
            if (request is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.NotFound, localizer[nameof(OrderingError.NotFound)]);
            }

            if (command.ActorRole != "PROVIDER" || !command.ActorCompanyId.HasValue || request.ProviderId != command.ActorCompanyId.Value)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError, "Only the assigned provider company can approve this fuel request.");
            }

            if (request.Status != "PENDING")
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError, "Only pending fuel requests can be approved.");
            }

            var existingOrder = await repository.FindByRequestIdAsync(request.Id, cancellationToken);
            if (existingOrder is not null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError, "An order has already been created for this fuel request.");
            }

            // Authoritative fuel price derived strictly from InventoryItem
            var pricePerLiter = await inventoryContextFacade.FetchPricePerLiterAsync(request.ProviderId.Value, request.FuelType, cancellationToken);
            if (!pricePerLiter.HasValue || pricePerLiter.Value <= 0)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError,
                    "Provider does not have an active inventory price configured for this fuel type.");
            }

            decimal unitPrice = FuelUnitConverter.CalculateUnitPrice(pricePerLiter.Value, request.Unit);
            var totalAmount = Math.Round(unitPrice * request.Quantity, 2, MidpointRounding.AwayFromZero);

            // Reserve stock with unit conversion in Inventory
            var reserved = await inventoryContextFacade.ReserveStockAsync(
                request.ProviderId.Value,
                request.FuelType,
                request.Quantity,
                request.Unit,
                cancellationToken);

            if (!reserved)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError, "Insufficient inventory stock to approve request.");
            }

            request.Approve();
            requestRepository.Update(request);

            var order = new Order(new CreateOrderCommand(
                request.Id,
                request.BuyerCompanyId,
                request.ProviderId,
                request.EquipmentId,
                request.FuelType,
                request.Quantity,
                request.Unit,
                unitPrice,
                totalAmount,
                request.DeliveryAddress,
                "ACCEPTED",
                "PENDING",
                null,
                null,
                request.DeliveryDate,
                null,
                null,
                null,
                null,
                null,
                null));

            await repository.AddAsync(order, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            var notified = await notificationContextFacade.CreateNotificationAsync(
                "BUYER",
                request.BuyerCompanyId,
                request.ProviderId,
                "ORDER_ACCEPTED",
                "Fuel Request Approved",
                $"Your fuel request #{request.Id} has been approved. Order #{order.Id} created.",
                order.Id,
                $"/ordering/my-orders/{order.Id}",
                cancellationToken);

            if (!notified)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError,
                    "Failed to create notification for buyer.");
            }

            await eventBus.PublishAsync(new RequestApprovedEvent(
                request.Id,
                order.Id,
                request.BuyerCompanyId,
                request.ProviderId,
                request.Quantity,
                request.FuelType), cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<Order>.Success(order);
        }
        catch (DbUpdateConcurrencyException)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.OperationCancelled,
                "The operation conflicted with concurrent changes. Please retry.");
        }
        catch (ArgumentException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.ValidationError, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.ValidationError, ex.Message);
        }
        catch (DbUpdateException)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.DatabaseError,
                "A database error occurred while processing the request.");
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.InternalServerError,
                "An unexpected internal error occurred while processing the request.");
        }
    }

    public async Task<Result<Order>> Handle(DispatchOrderCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            var order = await repository.FindByIdAsync(command.OrderId, cancellationToken);
            if (order is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.NotFound, localizer[nameof(OrderingError.NotFound)]);
            }

            if (command.ActorRole != "PROVIDER" || !command.ActorCompanyId.HasValue || order.ProviderId != command.ActorCompanyId.Value)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError, "Only the owning provider company can dispatch this order.");
            }

            if (order.Status != "ACCEPTED")
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError, $"Cannot dispatch order in '{order.Status}' status. Order must be in 'ACCEPTED' status.");
            }

            // Create delivery and assign driver/vehicle in Fulfillment
            var deliveryCreated = await fulfillmentContextFacade.CreateDeliveryAsync(
                order.Id,
                order.ProviderId.Value,
                command.DriverId,
                command.VehicleId,
                order.DeliveryAddress,
                order.Quantity,
                order.Unit,
                null,
                cancellationToken);

            if (!deliveryCreated)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError,
                    "Failed to assign driver or vehicle. Please ensure both are available and belong to your company.");
            }

            // Fulfill stock reservation in Inventory with explicit unit (creates OUT movement)
            var stockFulfilled = await inventoryContextFacade.FulfillStockAsync(
                order.ProviderId.Value,
                order.FuelType,
                order.Quantity,
                order.Unit,
                order.Id,
                cancellationToken);

            if (!stockFulfilled)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError,
                    "Failed to fulfill inventory stock reservation.");
            }

            order.Dispatch(command.DriverId, command.VehicleId);
            repository.Update(order);
            await unitOfWork.CompleteAsync(cancellationToken);

            var notified = await notificationContextFacade.CreateNotificationAsync(
                "BUYER",
                order.BuyerCompanyId,
                order.ProviderId,
                "ORDER_DISPATCHED",
                "Order Dispatched",
                $"Order #{order.Id} has been dispatched to {order.DeliveryAddress}.",
                order.Id,
                $"/ordering/my-orders/{order.Id}",
                cancellationToken);

            if (!notified)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError,
                    "Failed to create notification for buyer.");
            }

            await eventBus.PublishAsync(new OrderDispatchedEvent(
                order.Id,
                order.BuyerCompanyId,
                order.ProviderId,
                command.DriverId,
                command.VehicleId), cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<Order>.Success(order);
        }
        catch (DbUpdateConcurrencyException)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.OperationCancelled,
                "The operation conflicted with concurrent changes. Please retry.");
        }
        catch (ArgumentException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.ValidationError, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.ValidationError, ex.Message);
        }
        catch (DbUpdateException)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.DatabaseError,
                "A database error occurred while processing the request.");
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.InternalServerError,
                "An unexpected internal error occurred while processing the request.");
        }
    }

    public async Task<Result<Order>> Handle(ReceiveOrderCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            var order = await repository.FindByIdAsync(command.OrderId, cancellationToken);
            if (order is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.NotFound, localizer[nameof(OrderingError.NotFound)]);
            }

            if (command.ActorRole != "BUYER" || !command.ActorCompanyId.HasValue || order.BuyerCompanyId != command.ActorCompanyId.Value)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError, "Only the buyer company that placed this order can confirm receipt.");
            }

            if (order.Status != "DISPATCHED")
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError, $"Cannot receive order in '{order.Status}' status. Order must be in 'DISPATCHED' status.");
            }

            // Complete delivery and free fulfillment resources (driver and vehicle)
            var deliveryCompleted = await fulfillmentContextFacade.CompleteDeliveryAsync(order.Id, cancellationToken);
            if (!deliveryCompleted)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError, "Failed to complete delivery and release fulfillment fleet.");
            }

            // Refill equipment if linked with unit and fuel compatibility checks
            if (order.EquipmentId.HasValue && order.BuyerCompanyId.HasValue)
            {
                var refilled = await equipmentContextFacade.RefillEquipmentAsync(
                    order.EquipmentId.Value,
                    order.BuyerCompanyId.Value,
                    order.Quantity,
                    order.Unit,
                    order.FuelType,
                    order.ProviderId,
                    order.RequestId,
                    cancellationToken);

                if (!refilled)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<Order>.Failure(OrderingError.ValidationError, "Failed to refill buyer equipment.");
                }
            }

            order.Receive();
            repository.Update(order);
            await unitOfWork.CompleteAsync(cancellationToken);

            var notified = await notificationContextFacade.CreateNotificationAsync(
                "PROVIDER",
                order.BuyerCompanyId,
                order.ProviderId,
                "ORDER_RECEIVED",
                "Order Received",
                $"Order #{order.Id} has been received and confirmed by the buyer. Pending payment.",
                order.Id,
                $"/ordering/orders/{order.Id}",
                cancellationToken);

            if (!notified)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError,
                    "Failed to create notification for provider.");
            }

            await eventBus.PublishAsync(new OrderReceivedEvent(
                order.Id,
                order.BuyerCompanyId,
                order.ProviderId,
                order.EquipmentId), cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<Order>.Success(order);
        }
        catch (DbUpdateConcurrencyException)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.OperationCancelled,
                "The operation conflicted with concurrent changes. Please retry.");
        }
        catch (ArgumentException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.ValidationError, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.ValidationError, ex.Message);
        }
        catch (DbUpdateException)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.DatabaseError,
                "A database error occurred while processing the request.");
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.InternalServerError,
                "An unexpected internal error occurred while processing the request.");
        }
    }

    public async Task<Result<Order>> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            var order = await repository.FindByIdAsync(command.OrderId, cancellationToken);
            if (order is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.NotFound, localizer[nameof(OrderingError.NotFound)]);
            }

            // Cancellation rule: Only ACCEPTED orders can be cancelled before dispatch by the owning PROVIDER
            if (command.ActorRole != "PROVIDER" || !command.ActorCompanyId.HasValue || order.ProviderId != command.ActorCompanyId.Value)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError,
                    "Only the owning provider company can cancel an order prior to dispatch.");
            }

            if (order.Status != "ACCEPTED")
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError,
                    $"Cannot cancel order in '{order.Status}' status. Only orders in 'ACCEPTED' status can be cancelled prior to dispatch.");
            }

            // Release reserved inventory stock with explicit unit
            if (order.ProviderId.HasValue)
            {
                var released = await inventoryContextFacade.ReleaseStockAsync(
                    order.ProviderId.Value,
                    order.FuelType,
                    order.Quantity,
                    order.Unit,
                    cancellationToken);

                if (!released)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<Order>.Failure(OrderingError.ValidationError,
                        "Failed to release reserved inventory stock.");
                }
            }

            var reason = string.IsNullOrWhiteSpace(command.Reason) ? "Cancelled by provider" : command.Reason;
            order.Cancel(reason);
            repository.Update(order);
            await unitOfWork.CompleteAsync(cancellationToken);

            var notified = await notificationContextFacade.CreateNotificationAsync(
                "BUYER",
                order.BuyerCompanyId,
                order.ProviderId,
                "ORDER_CANCELLED",
                "Order Cancelled",
                $"Order #{order.Id} was cancelled by the provider. Reason: {reason}",
                order.Id,
                $"/ordering/my-orders/{order.Id}",
                cancellationToken);

            if (!notified)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Order>.Failure(OrderingError.ValidationError,
                    "Failed to create notification for buyer.");
            }

            await eventBus.PublishAsync(new OrderCancelledEvent(
                order.Id,
                order.BuyerCompanyId,
                order.ProviderId,
                reason), cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<Order>.Success(order);
        }
        catch (DbUpdateConcurrencyException)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.OperationCancelled,
                "The operation conflicted with concurrent changes. Please retry.");
        }
        catch (ArgumentException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.ValidationError, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.ValidationError, ex.Message);
        }
        catch (DbUpdateException)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.DatabaseError,
                "A database error occurred while processing the request.");
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<Order>.Failure(OrderingError.InternalServerError,
                "An unexpected internal error occurred while processing the request.");
        }
    }
}
