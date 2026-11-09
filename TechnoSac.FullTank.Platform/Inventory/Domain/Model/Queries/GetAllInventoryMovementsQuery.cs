namespace TechnoSac.FullTank.Platform.Inventory.Domain.Model.Queries;

/// <summary>Query to get inventory movements, optionally filtered by inventory item id.</summary>
public record GetAllInventoryMovementsQuery(int? InventoryItemId = null);
