using TechnoSac.FullTank.Platform.Shared.Domain.Model.Events;

namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Events;

public record OrderDispatchedEvent(
    int OrderId,
    int? BuyerCompanyId,
    int? ProviderId,
    int DriverId,
    int VehicleId) : IEvent;
