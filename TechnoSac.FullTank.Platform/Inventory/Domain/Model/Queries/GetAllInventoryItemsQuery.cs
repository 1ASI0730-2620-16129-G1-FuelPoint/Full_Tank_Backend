namespace TechnoSac.FullTank.Platform.Inventory.Domain.Model.Queries;

/// <summary>Query to get inventory items, optionally filtered by provider id.</summary>
public record GetAllInventoryItemsQuery(int? ProviderId = null);
