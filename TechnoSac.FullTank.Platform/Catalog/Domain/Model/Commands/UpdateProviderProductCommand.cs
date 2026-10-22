namespace TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;

public record UpdateProviderProductCommand(
    int Id,
    int? ProviderId,
    string FuelType,
    string Name,
    string Description,
    decimal PricePerLiter,
    string Unit,
    bool Available);
