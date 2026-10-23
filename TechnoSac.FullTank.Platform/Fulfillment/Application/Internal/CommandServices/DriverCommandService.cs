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

public class DriverCommandService(
    IDriverRepository repository,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IDriverCommandService
{
    public async Task<Result<Driver>> Handle(CreateDriverCommand command, CancellationToken cancellationToken)
    {
        var driver = new Driver(command);
        try
        {
            await repository.AddAsync(driver, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Driver>.Success(driver);
        }
        catch (DbUpdateException)
        {
            return Result<Driver>.Failure(FulfillmentError.DatabaseError,
                localizer[nameof(FulfillmentError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<Driver>.Failure(FulfillmentError.InternalServerError,
                localizer[nameof(FulfillmentError.InternalServerError)]);
        }
    }

    public async Task<Result<Driver>> Handle(UpdateDriverCommand command, CancellationToken cancellationToken)
    {
        var driver = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (driver is null)
            return Result<Driver>.Failure(FulfillmentError.NotFound, localizer[nameof(FulfillmentError.NotFound)]);

        try
        {
            driver.Update(command);
            repository.Update(driver);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Driver>.Success(driver);
        }
        catch (Exception)
        {
            return Result<Driver>.Failure(FulfillmentError.InternalServerError,
                localizer[nameof(FulfillmentError.InternalServerError)]);
        }
    }
}
