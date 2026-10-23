namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;

/// <summary>Query to get drivers, optionally filtered by provider id.</summary>
public record GetAllDriversQuery(int? ProviderId = null);
