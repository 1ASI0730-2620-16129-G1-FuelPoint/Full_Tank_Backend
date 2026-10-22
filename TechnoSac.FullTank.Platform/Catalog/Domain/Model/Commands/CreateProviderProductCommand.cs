namespace TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;

public record CreateProviderProductCommand(
    int? ProviderId,
    string FuelType,
    string Name,
    string Description,
    decimal PricePerLiter,
    string Unit,
    bool Available);
