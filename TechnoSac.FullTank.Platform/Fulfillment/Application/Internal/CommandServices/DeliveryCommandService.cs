using TechnoSac.FullTank.Platform.Fulfillment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.Internal.CommandServices;

public class DeliveryCommandService(
    IDeliveryRepository repository,
    IDriverRepository driverRepository,
    IVehicleRepository vehicleRepository,
    IOrderingContextFacade orderingContextFacade,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IDeliveryCommandService
{
    public Task<Result<Delivery>> Handle(CreateDeliveryCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<Delivery>.Failure(FulfillmentError.ValidationError,
            "Direct delivery creation is disabled. Use order workflow dispatch transition: POST /api/v1/orders/{id}/dispatch."));
    }

    public async Task<Result<Delivery>> Handle(UpdateDeliveryCommand command, CancellationToken cancellationToken)
    {
        var delivery = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (delivery is null)
            return Result<Delivery>.Failure(FulfillmentError.NotFound, localizer[nameof(FulfillmentError.NotFound)]);

        try
        {
            delivery.Update(command);
            repository.Update(delivery);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Delivery>.Success(delivery);
        }
        catch (Exception)
        {
            return Result<Delivery>.Failure(FulfillmentError.InternalServerError,
                localizer[nameof(FulfillmentError.InternalServerError)]);
        }
    }

    public Task<Result<Delivery>> Handle(CompleteDeliveryCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<Delivery>.Failure(FulfillmentError.ValidationError,
            "Direct delivery completion is disabled. Use order workflow receive transition: POST /api/v1/orders/{id}/receive."));
    }

    private Result<Delivery> ValidationFailure()
    {
        return Result<Delivery>.Failure(FulfillmentError.ValidationError,
            localizer[nameof(FulfillmentError.ValidationError)]);
    }
}
