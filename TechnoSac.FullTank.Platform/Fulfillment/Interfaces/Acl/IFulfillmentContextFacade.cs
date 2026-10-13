namespace TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Acl;

public interface IFulfillmentContextFacade
{
    Task<bool> CreateDeliveryAsync(int orderId, int providerId, int driverId, int vehicleId, string destination,
        int quantity, string unit, string? originLocation = null, CancellationToken cancellationToken = default);

    Task<bool> CreateDeliveryAsync(int orderId, int providerId, int driverId, int vehicleId, string destination,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteDeliveryAsync(int orderId, CancellationToken cancellationToken = default);

    Task<bool> CancelDeliveryAsync(int orderId, CancellationToken cancellationToken = default);
}

