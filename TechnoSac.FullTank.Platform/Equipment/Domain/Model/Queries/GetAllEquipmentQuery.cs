namespace TechnoSac.FullTank.Platform.Equipment.Domain.Model.Queries;

/// <summary>Query to get equipment, optionally filtered by buyer company id.</summary>
public record GetAllEquipmentQuery(int? CompanyId = null);
