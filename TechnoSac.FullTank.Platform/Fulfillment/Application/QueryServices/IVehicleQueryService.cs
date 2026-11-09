using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.QueryServices;

public interface IVehicleQueryService
{
    Task<IEnumerable<Vehicle>> Handle(GetAllVehiclesQuery query, CancellationToken cancellationToken);
    Task<Vehicle?> Handle(GetVehicleByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<Vehicle>> Handle(GetAvailableVehiclesByProviderQuery query, CancellationToken cancellationToken);
}
