using TechnoSac.FullTank.Platform.Equipment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Equipment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Equipment.Application.Internal.CommandServices;

public class RefillHistoryCommandService(
    IRefillHistoryRepository repository,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IRefillHistoryCommandService
{
    public async Task<Result<RefillHistory>> Handle(CreateRefillHistoryCommand command,
        CancellationToken cancellationToken)
    {
        var refill = new RefillHistory(command);
        try
        {
            await repository.AddAsync(refill, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<RefillHistory>.Success(refill);
        }
        catch (Exception)
        {
            return Result<RefillHistory>.Failure(EquipmentError.InternalServerError,
                localizer[nameof(EquipmentError.InternalServerError)]);
        }
    }
}
