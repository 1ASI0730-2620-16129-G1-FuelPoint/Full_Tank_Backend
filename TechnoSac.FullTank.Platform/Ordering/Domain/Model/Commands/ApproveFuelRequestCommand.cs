namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;

public record ApproveFuelRequestCommand(int RequestId, string ActorRole, int? ActorCompanyId);
