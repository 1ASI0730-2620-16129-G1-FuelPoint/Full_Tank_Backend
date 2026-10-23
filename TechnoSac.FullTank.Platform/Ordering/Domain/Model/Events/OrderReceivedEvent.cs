using TechnoSac.FullTank.Platform.Shared.Domain.Model.Events;

namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Events;

public record OrderReceivedEvent(
    int OrderId,
    int? BuyerCompanyId,
    int? ProviderId,
    int? EquipmentId) : IEvent;
