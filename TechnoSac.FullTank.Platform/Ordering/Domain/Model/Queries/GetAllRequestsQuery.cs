namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model.Queries;

/// <summary>Query to get requests, optionally filtered by buyer company or provider.</summary>
public record GetAllRequestsQuery(int? BuyerCompanyId = null, int? ProviderId = null);
