using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.CommandServices;

public interface IDriverCommandService
{
    Task<Result<Driver>> Handle(CreateDriverCommand command, CancellationToken cancellationToken);
    Task<Result<Driver>> Handle(UpdateDriverCommand command, CancellationToken cancellationToken);
}
