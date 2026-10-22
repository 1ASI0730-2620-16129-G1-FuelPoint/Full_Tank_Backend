using TechnoSac.FullTank.Platform.Inventory.Application.CommandServices;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Domain.Repositories;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Inventory.Application.Internal.CommandServices;

public class InventoryMovementCommandService(
    IInventoryMovementRepository repository,
    IInventoryItemRepository inventoryItemRepository,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IInventoryMovementCommandService
{
    public async Task<Result<InventoryMovement>> Handle(CreateInventoryMovementCommand command,
        CancellationToken cancellationToken)
    {
        if (command.InventoryItemId is not > 0
            || command.ProviderId is not > 0
            || command.Quantity <= 0
            || command.Type is not ("IN" or "OUT")
            || string.IsNullOrWhiteSpace(command.Reason))
            return ValidationFailure();

        var inventoryItem = await inventoryItemRepository.FindByIdAsync(command.InventoryItemId.Value,
            cancellationToken);
        if (inventoryItem is null
            || inventoryItem.ProviderId != command.ProviderId
            || !await iamContextFacade.ExistsProviderCompany(command.ProviderId.Value, cancellationToken))
            return ValidationFailure();

        var movement = new InventoryMovement(command);
        try
        {
            await repository.AddAsync(movement, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<InventoryMovement>.Success(movement);
        }
        catch (Exception)
        {
            return Result<InventoryMovement>.Failure(InventoryError.InternalServerError,
                localizer[nameof(InventoryError.InternalServerError)]);
        }
    }

    private Result<InventoryMovement> ValidationFailure()
    {
        return Result<InventoryMovement>.Failure(InventoryError.ValidationError,
            localizer[nameof(InventoryError.ValidationError)]);
    }
}
