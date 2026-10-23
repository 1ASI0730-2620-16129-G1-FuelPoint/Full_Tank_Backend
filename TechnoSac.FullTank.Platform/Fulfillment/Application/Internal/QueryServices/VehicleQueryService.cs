using TechnoSac.FullTank.Platform.Fulfillment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.Internal.QueryServices;

public class VehicleQueryService(IVehicleRepository repository) : IVehicleQueryService
{
    public async Task<IEnumerable<Vehicle>> Handle(GetAllVehiclesQuery query, CancellationToken cancellationToken)
    {
        return query.ProviderId is null
            ? await repository.ListAsync(cancellationToken)
            : await repository.FindByProviderIdAsync(query.ProviderId.Value, cancellationToken);
    }

    public async Task<Vehicle?> Handle(GetVehicleByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<Vehicle>> Handle(GetAvailableVehiclesByProviderQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.FindAvailableByProviderIdAsync(query.ProviderId, cancellationToken);
    }
}
