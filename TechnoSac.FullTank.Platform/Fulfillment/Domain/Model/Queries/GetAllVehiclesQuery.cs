namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;

/// <summary>Query to get vehicles, optionally filtered by provider id.</summary>
public record GetAllVehiclesQuery(int? ProviderId = null);
