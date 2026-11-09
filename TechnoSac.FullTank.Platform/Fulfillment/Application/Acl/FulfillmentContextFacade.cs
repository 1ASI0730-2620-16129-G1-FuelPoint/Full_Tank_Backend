using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Domain.Services;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.Acl;

public class FulfillmentContextFacade(
    IDeliveryRepository deliveryRepository,
    IDriverRepository driverRepository,
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork,
    IIamContextFacade? iamContextFacade = null) : IFulfillmentContextFacade
{
    public async Task<bool> CreateDeliveryAsync(int orderId, int providerId, int driverId, int vehicleId,
        string destination, int quantity, string unit, string? originLocation = null,
        CancellationToken cancellationToken = default)
    {
        // Check existing active delivery for this order
        var existingActive = await deliveryRepository.FindActiveByOrderIdAsync(orderId, cancellationToken);
        if (existingActive is not null) return false;

        var driver = await driverRepository.FindByIdAsync(driverId, cancellationToken);
        var vehicle = await vehicleRepository.FindByIdAsync(vehicleId, cancellationToken);

        if (driver is null || vehicle is null) return false;
        if (driver.ProviderId != providerId || vehicle.ProviderId != providerId) return false;

        // Require AVAILABLE, reject busy, assigned, inactive, maintenance
        if (!string.Equals(driver.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(vehicle.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase))
            return false;

        // Check vehicle capacity with order unit converted
        if (quantity > 0)
        {
            int requiredVehicleUnits;
            try
            {
                requiredVehicleUnits = FuelUnitConverter.ConvertToEquipmentUnits(quantity, unit, vehicle.Unit);
            }
            catch (OverflowException)
            {
                return false;
            }

            if (vehicle.Capacity < requiredVehicleUnits)
                return false;
        }

        // Use company location via existing ACL where available, or passed origin; no hardcoded invented origin
        string origin = destination;
        if (!string.IsNullOrWhiteSpace(originLocation))
        {
            origin = originLocation;
        }
        else if (iamContextFacade is not null)
        {
            var (companyName, _) = await iamContextFacade.FetchProviderCompanyInfo(providerId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(companyName))
                origin = companyName;
        }

        try
        {
            driver.SetStatus("ASSIGNED");
            driverRepository.Update(driver);

            vehicle.SetStatus("ASSIGNED");
            vehicleRepository.Update(vehicle);

            var delivery = new Delivery(new CreateDeliveryCommand(
                orderId,
                providerId,
                driverId,
                vehicleId,
                "on_the_way",
                origin,
                destination,
                DateTimeOffset.UtcNow.ToString("O"),
                null,
                null));

            await deliveryRepository.AddAsync(delivery, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Task<bool> CreateDeliveryAsync(int orderId, int providerId, int driverId, int vehicleId,
        string destination, CancellationToken cancellationToken = default)
    {
        return CreateDeliveryAsync(orderId, providerId, driverId, vehicleId, destination, 0, "LITERS", null, cancellationToken);
    }

    public async Task<bool> CompleteDeliveryAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var delivery = await deliveryRepository.FindActiveByOrderIdAsync(orderId, cancellationToken);
        if (delivery is null) return false; // Reject missing delivery to prevent phantom receipt

        try
        {
            delivery.Complete();
            deliveryRepository.Update(delivery);

            if (delivery.DriverId.HasValue)
            {
                var driver = await driverRepository.FindByIdAsync(delivery.DriverId.Value, cancellationToken);
                if (driver is not null)
                {
                    driver.SetStatus("AVAILABLE");
                    driverRepository.Update(driver);
                }
            }

            if (delivery.VehicleId.HasValue)
            {
                var vehicle = await vehicleRepository.FindByIdAsync(delivery.VehicleId.Value, cancellationToken);
                if (vehicle is not null)
                {
                    vehicle.SetStatus("AVAILABLE");
                    vehicleRepository.Update(vehicle);
                }
            }

            await unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> CancelDeliveryAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var delivery = await deliveryRepository.FindActiveByOrderIdAsync(orderId, cancellationToken);
        if (delivery is null) return true;

        try
        {
            delivery.Cancel();
            deliveryRepository.Update(delivery);

            if (delivery.DriverId.HasValue)
            {
                var driver = await driverRepository.FindByIdAsync(delivery.DriverId.Value, cancellationToken);
                if (driver is not null)
                {
                    driver.SetStatus("AVAILABLE");
                    driverRepository.Update(driver);
                }
            }

            if (delivery.VehicleId.HasValue)
            {
                var vehicle = await vehicleRepository.FindByIdAsync(delivery.VehicleId.Value, cancellationToken);
                if (vehicle is not null)
                {
                    vehicle.SetStatus("AVAILABLE");
                    vehicleRepository.Update(vehicle);
                }
            }

            await unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
