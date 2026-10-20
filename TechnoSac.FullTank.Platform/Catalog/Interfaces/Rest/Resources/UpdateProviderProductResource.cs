namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

public record UpdateProviderProductResource(
    int? ProviderId,
    string FuelType,
    string Name,
    string Description,
    decimal PricePerLiter,
    string Unit,
    bool Available);
