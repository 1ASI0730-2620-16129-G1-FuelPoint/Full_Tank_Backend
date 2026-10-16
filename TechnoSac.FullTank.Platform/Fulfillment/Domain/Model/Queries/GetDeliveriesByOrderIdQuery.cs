namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;

/// <summary>Query to get deliveries for a given order id.</summary>
public record GetDeliveriesByOrderIdQuery(int OrderId);
