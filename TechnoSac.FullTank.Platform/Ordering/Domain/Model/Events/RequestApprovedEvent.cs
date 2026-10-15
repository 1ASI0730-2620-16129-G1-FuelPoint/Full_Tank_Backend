using TechnoSac.FullTank.Platform.Shared.Domain.Model.Events;

namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Events;

public record RequestApprovedEvent(
    int RequestId,
    int OrderId,
    int? BuyerCompanyId,
    int? ProviderId,
    int Quantity,
    string FuelType) : IEvent;
