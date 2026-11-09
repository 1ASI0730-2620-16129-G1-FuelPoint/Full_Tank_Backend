namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;

/// <summary>Query to get the available vehicles for a given provider id.</summary>
public record GetAvailableVehiclesByProviderQuery(int ProviderId);
