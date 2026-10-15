using TechnoSac.FullTank.Platform.Ordering.Application.CommandServices;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Ordering.Domain.Repositories;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Ordering.Application.Internal.CommandServices;

public class RequestCommandService(
    IRequestRepository repository,
    IOrderRepository orderRepository,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IRequestCommandService
{
    public async Task<Result<Request>> Handle(CreateRequestCommand command, CancellationToken cancellationToken)
    {
        var status = string.IsNullOrWhiteSpace(command.Status) ? "PENDING" : command.Status;
        var source = string.IsNullOrWhiteSpace(command.Source) ? "MANUAL" : command.Source;
        if (!IsValid(command.BuyerCompanyId, command.ProviderId, command.FuelType, command.ProductName,
                command.Quantity, command.Unit, command.UnitPrice, command.DeliveryAddress, command.DeliveryDate,
                status, source)
            || status != "PENDING"
            || !await ReferencesExist(command.BuyerCompanyId, command.ProviderId, cancellationToken))
            return ValidationFailure();

        try
        {
            var request = new Request(command);
            await repository.AddAsync(request, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Request>.Success(request);
        }
        catch (ArgumentException ex)
        {
            return Result<Request>.Failure(OrderingError.ValidationError, ex.Message);
        }
        catch (DbUpdateException)
        {
            return Result<Request>.Failure(OrderingError.DatabaseError, localizer[nameof(OrderingError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<Request>.Failure(OrderingError.InternalServerError,
                localizer[nameof(OrderingError.InternalServerError)]);
        }
    }

    public async Task<Result<Request>> Handle(UpdateRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (request is null)
            return Result<Request>.Failure(OrderingError.NotFound, localizer[nameof(OrderingError.NotFound)]);
        if (!IsValid(command.BuyerCompanyId, command.ProviderId, command.FuelType, command.ProductName,
                command.Quantity, command.Unit, command.UnitPrice, command.DeliveryAddress, command.DeliveryDate,
                command.Status, command.Source)
            || !await ReferencesExist(command.BuyerCompanyId, command.ProviderId, cancellationToken)
            || !CanTransition(request.Status, command.Status)
            || (command.Status == "REJECTED" && string.IsNullOrWhiteSpace(command.RejectionReasonCode)))
            return ValidationFailure();

        try
        {
            request.Update(command);
            repository.Update(request);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Request>.Success(request);
        }
        catch (ArgumentException ex)
        {
            return Result<Request>.Failure(OrderingError.ValidationError, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Request>.Failure(OrderingError.ValidationError, ex.Message);
        }
        catch (Exception)
        {
            return Result<Request>.Failure(OrderingError.InternalServerError,
                localizer[nameof(OrderingError.InternalServerError)]);
        }
    }

    public async Task<Result> Handle(DeleteRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (request is null)
            return Result.Failure(OrderingError.NotFound, localizer[nameof(OrderingError.NotFound)]);
        if (await orderRepository.FindByRequestIdAsync(request.Id, cancellationToken) is not null)
            return Result.Failure(OrderingError.OperationCancelled,
                localizer[nameof(OrderingError.OperationCancelled)]);

        repository.Remove(request);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }

    private static bool IsValid(int? buyerCompanyId, int? providerId, string fuelType, string productName,
        int quantity, string unit, decimal unitPrice, string deliveryAddress, string deliveryDate, string status,
        string source)
    {
        return buyerCompanyId is > 0
               && providerId is > 0
               && !string.IsNullOrWhiteSpace(fuelType)
               && !string.IsNullOrWhiteSpace(productName)
               && quantity > 0
               && !string.IsNullOrWhiteSpace(unit)
               && unitPrice >= 0
               && !string.IsNullOrWhiteSpace(deliveryAddress)
               && !string.IsNullOrWhiteSpace(deliveryDate)
               && status is "PENDING" or "APPROVED" or "REJECTED"
               && source is "MANUAL" or "AUTO_REFILL";
    }

    private async Task<bool> ReferencesExist(int? buyerCompanyId, int? providerId,
        CancellationToken cancellationToken)
    {
        return buyerCompanyId.HasValue
               && providerId.HasValue
               && await iamContextFacade.ExistsBuyerCompany(buyerCompanyId.Value, cancellationToken)
               && await iamContextFacade.ExistsProviderCompany(providerId.Value, cancellationToken);
    }

    private static bool CanTransition(string current, string next)
    {
        return current == next || current == "PENDING" && next is "APPROVED" or "REJECTED";
    }

    private Result<Request> ValidationFailure()
    {
        return Result<Request>.Failure(OrderingError.ValidationError,
            localizer[nameof(OrderingError.ValidationError)]);
    }
}
