using TechnoSac.FullTank.Platform.Inventory.Application.CommandServices;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Domain.Repositories;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Inventory.Application.Internal.CommandServices;

public class InventoryItemCommandService(
    IInventoryItemRepository repository,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IInventoryItemCommandService
{
    public async Task<Result<InventoryItem>> Handle(CreateInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        if (!IsValid(command.ProviderId, command.Name, command.Type, command.PricePerLiter, command.Stock,
                command.Reserved, command.Capacity, command.LowStockThreshold, command.Unit, command.Status)
            || !await ProviderExists(command.ProviderId, cancellationToken))
            return ValidationFailure();

        try
        {
            var item = new InventoryItem(command);
            await repository.AddAsync(item, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<InventoryItem>.Success(item);
        }
        catch (ArgumentException ex)
        {
            return Result<InventoryItem>.Failure(InventoryError.ValidationError, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Result<InventoryItem>.Failure(InventoryError.ValidationError, ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<InventoryItem>.Failure(InventoryError.OperationCancelled, "Concurrent stock modification detected. Please retry.");
        }
        catch (DbUpdateException)
        {
            return Result<InventoryItem>.Failure(InventoryError.DatabaseError,
                localizer[nameof(InventoryError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<InventoryItem>.Failure(InventoryError.InternalServerError,
                localizer[nameof(InventoryError.InternalServerError)]);
        }
    }

    public async Task<Result<InventoryItem>> Handle(UpdateInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (item is null)
            return Result<InventoryItem>.Failure(InventoryError.NotFound, localizer[nameof(InventoryError.NotFound)]);

        var providerId = item.ProviderId;
        if (!IsValid(providerId, command.Name, command.Type, command.PricePerLiter, command.Stock,
                command.Reserved, command.Capacity, command.LowStockThreshold, command.Unit, command.Status)
            || !await ProviderExists(providerId, cancellationToken))
            return ValidationFailure();

        try
        {
            item.Update(command);
            repository.Update(item);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<InventoryItem>.Success(item);
        }
        catch (ArgumentException ex)
        {
            return Result<InventoryItem>.Failure(InventoryError.ValidationError, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Result<InventoryItem>.Failure(InventoryError.ValidationError, ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<InventoryItem>.Failure(InventoryError.OperationCancelled, "Concurrent stock modification detected. Please retry.");
        }
        catch (Exception)
        {
            return Result<InventoryItem>.Failure(InventoryError.InternalServerError,
                localizer[nameof(InventoryError.InternalServerError)]);
        }
    }

    public async Task<Result> Handle(DeleteInventoryItemCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (item is null)
            return Result.Failure(InventoryError.NotFound, localizer[nameof(InventoryError.NotFound)]);

        try
        {
            repository.Remove(item);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException)
        {
            return Result.Failure(InventoryError.DatabaseError, localizer[nameof(InventoryError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result.Failure(InventoryError.InternalServerError,
                localizer[nameof(InventoryError.InternalServerError)]);
        }
    }

    private static bool IsValid(int? providerId, string name, string type, decimal pricePerLiter, int stock,
        int reserved, int capacity, int lowStockThreshold, string unit, string status)
    {
        return providerId is > 0
               && !string.IsNullOrWhiteSpace(name)
               && !string.IsNullOrWhiteSpace(type)
               && pricePerLiter >= 0
               && stock >= 0
               && reserved >= 0
               && reserved <= stock
               && capacity > 0
               && stock <= capacity
               && lowStockThreshold >= 0
               && lowStockThreshold <= capacity
               && !string.IsNullOrWhiteSpace(unit)
               && status is "ACTIVE" or "DISABLED";
    }

    private async Task<bool> ProviderExists(int? providerId, CancellationToken cancellationToken)
    {
        return providerId.HasValue
               && await iamContextFacade.ExistsProviderCompany(providerId.Value, cancellationToken);
    }

    private Result<InventoryItem> ValidationFailure()
    {
        return Result<InventoryItem>.Failure(InventoryError.ValidationError,
            localizer[nameof(InventoryError.ValidationError)]);
    }
}
