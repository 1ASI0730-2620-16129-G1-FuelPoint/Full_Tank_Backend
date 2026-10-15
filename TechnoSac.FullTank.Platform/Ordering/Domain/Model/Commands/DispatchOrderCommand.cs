namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;

public record DispatchOrderCommand(int OrderId, int DriverId, int VehicleId, string ActorRole, int? ActorCompanyId);
