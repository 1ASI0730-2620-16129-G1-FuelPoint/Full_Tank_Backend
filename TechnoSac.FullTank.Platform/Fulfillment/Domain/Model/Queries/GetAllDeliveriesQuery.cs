namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;

/// <summary>Query to get deliveries, optionally filtered by provider id.</summary>
public record GetAllDeliveriesQuery(int? ProviderId = null);
