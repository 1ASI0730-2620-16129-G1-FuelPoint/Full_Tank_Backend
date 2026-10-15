namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;

public record ReceiveOrderCommand(int OrderId, string ActorRole, int? ActorCompanyId);
