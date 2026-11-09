namespace TechnoSac.FullTank.Platform.Catalog.Domain.Model.Queries;

/// <summary>Query to get favorite providers, optionally filtered by buyer company id.</summary>
public record GetAllFavoriteProvidersQuery(int? CompanyId = null);
