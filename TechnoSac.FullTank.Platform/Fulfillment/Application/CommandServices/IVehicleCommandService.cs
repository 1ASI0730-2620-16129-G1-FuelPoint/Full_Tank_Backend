using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.CommandServices;

public interface IVehicleCommandService
{
    Task<Result<Vehicle>> Handle(CreateVehicleCommand command, CancellationToken cancellationToken);
    Task<Result<Vehicle>> Handle(UpdateVehicleCommand command, CancellationToken cancellationToken);
}
