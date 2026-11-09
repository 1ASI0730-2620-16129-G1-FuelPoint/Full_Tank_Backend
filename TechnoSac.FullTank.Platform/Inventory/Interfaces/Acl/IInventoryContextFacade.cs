namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Acl;

public interface IInventoryContextFacade
{
    Task<decimal?> FetchPricePerLiterAsync(int providerId, string fuelType, CancellationToken cancellationToken);
    Task<bool> ReserveStockAsync(int providerId, string fuelType, int quantity, string unit = "LITERS", CancellationToken cancellationToken = default);
    Task<bool> ReleaseStockAsync(int providerId, string fuelType, int quantity, string unit = "LITERS", CancellationToken cancellationToken = default);
    Task<bool> FulfillStockAsync(int providerId, string fuelType, int quantity, string unit, int orderId, CancellationToken cancellationToken = default);
    Task<bool> FulfillStockAsync(int providerId, string fuelType, int quantity, int orderId, CancellationToken cancellationToken = default);
}
