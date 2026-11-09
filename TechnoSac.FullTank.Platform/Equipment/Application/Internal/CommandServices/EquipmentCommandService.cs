using TechnoSac.FullTank.Platform.Equipment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Equipment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;

namespace TechnoSac.FullTank.Platform.Equipment.Application.Internal.CommandServices;

public class EquipmentCommandService(
    IEquipmentRepository repository,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IEquipmentCommandService
{
    public async Task<Result<EquipmentAggregate>> Handle(CreateEquipmentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var equipment = new EquipmentAggregate(command);
            await repository.AddAsync(equipment, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<EquipmentAggregate>.Success(equipment);
        }
        catch (ArgumentException ex)
        {
            return Result<EquipmentAggregate>.Failure(EquipmentError.ValidationError, ex.Message);
        }
        catch (DbUpdateException)
        {
            return Result<EquipmentAggregate>.Failure(EquipmentError.DatabaseError,
                localizer[nameof(EquipmentError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<EquipmentAggregate>.Failure(EquipmentError.InternalServerError,
                localizer[nameof(EquipmentError.InternalServerError)]);
        }
    }

    public async Task<Result<EquipmentAggregate>> Handle(UpdateEquipmentCommand command,
        CancellationToken cancellationToken)
    {
        var equipment = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (equipment is null)
            return Result<EquipmentAggregate>.Failure(EquipmentError.NotFound,
                localizer[nameof(EquipmentError.NotFound)]);

        try
        {
            equipment.Update(command);
            repository.Update(equipment);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<EquipmentAggregate>.Success(equipment);
        }
        catch (ArgumentException ex)
        {
            return Result<EquipmentAggregate>.Failure(EquipmentError.ValidationError, ex.Message);
        }
        catch (DbUpdateException)
        {
            return Result<EquipmentAggregate>.Failure(EquipmentError.DatabaseError,
                localizer[nameof(EquipmentError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<EquipmentAggregate>.Failure(EquipmentError.InternalServerError,
                localizer[nameof(EquipmentError.InternalServerError)]);
        }
    }
}
