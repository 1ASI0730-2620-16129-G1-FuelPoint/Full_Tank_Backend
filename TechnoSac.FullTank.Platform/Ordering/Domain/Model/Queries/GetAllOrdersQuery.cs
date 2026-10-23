namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Queries;

/// <summary>Query to get orders, optionally filtered by buyer company or provider.</summary>
public record GetAllOrdersQuery(int? BuyerCompanyId = null, int? ProviderId = null);
