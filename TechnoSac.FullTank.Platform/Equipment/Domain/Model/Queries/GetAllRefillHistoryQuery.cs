namespace TechnoSac.FullTank.Platform.Equipment.Domain.Model.Queries;

/// <summary>Query to get refill history, optionally filtered by equipment id.</summary>
public record GetAllRefillHistoryQuery(int? EquipmentId = null);
