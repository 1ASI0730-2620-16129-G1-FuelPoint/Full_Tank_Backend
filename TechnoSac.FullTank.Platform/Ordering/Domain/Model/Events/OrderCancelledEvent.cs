using TechnoSac.FullTank.Platform.Shared.Domain.Model.Events;

namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Events;

public record OrderCancelledEvent(
    int OrderId,
    int? BuyerCompanyId,
    int? ProviderId,
    string Reason) : IEvent;
