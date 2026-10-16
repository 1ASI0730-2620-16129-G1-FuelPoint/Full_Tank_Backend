using TechnoSac.FullTank.Platform.Fulfillment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.Internal.CommandServices;

public class VehicleCommandService(
    IVehicleRepository repository,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IVehicleCommandService
{
    public async Task<Result<Vehicle>> Handle(CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        var vehicle = new Vehicle(command);
        try
        {
            await repository.AddAsync(vehicle, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Vehicle>.Success(vehicle);
        }
        catch (DbUpdateException)
        {
            return Result<Vehicle>.Failure(FulfillmentError.DatabaseError,
                localizer[nameof(FulfillmentError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<Vehicle>.Failure(FulfillmentError.InternalServerError,
                localizer[nameof(FulfillmentError.InternalServerError)]);
        }
    }

    public async Task<Result<Vehicle>> Handle(UpdateVehicleCommand command, CancellationToken cancellationToken)
    {
        var vehicle = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (vehicle is null)
            return Result<Vehicle>.Failure(FulfillmentError.NotFound, localizer[nameof(FulfillmentError.NotFound)]);

        try
        {
            vehicle.Update(command);
            repository.Update(vehicle);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Vehicle>.Success(vehicle);
        }
        catch (Exception)
        {
            return Result<Vehicle>.Failure(FulfillmentError.InternalServerError,
                localizer[nameof(FulfillmentError.InternalServerError)]);
        }
    }
}
