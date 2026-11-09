namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

public record CreateProviderProductResource(
    int? ProviderId,
    string FuelType,
    string Name,
    string Description,
    decimal PricePerLiter,
    string Unit,
    bool Available);
