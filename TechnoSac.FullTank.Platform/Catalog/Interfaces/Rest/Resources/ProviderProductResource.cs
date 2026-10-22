namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

public record ProviderProductResource(
    int Id,
    int? ProviderId,
    string FuelType,
    string Name,
    string Description,
    decimal PricePerLiter,
    string Unit,
    bool Available,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
