namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;

public record CancelOrderCommand(int OrderId, string Reason, string ActorRole, int? ActorCompanyId);
