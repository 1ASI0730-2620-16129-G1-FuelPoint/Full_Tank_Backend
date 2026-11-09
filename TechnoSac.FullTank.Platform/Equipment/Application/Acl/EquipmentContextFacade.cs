using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Equipment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Domain.Services;

namespace TechnoSac.FullTank.Platform.Equipment.Application.Acl;

public class EquipmentContextFacade(
    IEquipmentRepository equipmentRepository,
    IRefillHistoryRepository refillHistoryRepository,
    IUnitOfWork unitOfWork) : IEquipmentContextFacade
{
    public async Task<bool> RefillEquipmentAsync(int equipmentId, int companyId, int quantity, string unit, string fuelType,
        int? providerId, int? requestId, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) return false;

        var equipment = await equipmentRepository.FindByIdAsync(equipmentId, cancellationToken);
        if (equipment is null || equipment.CompanyId != companyId)
            return false;

        // Reject genuinely inactive state
        if (string.Equals(equipment.Status, "INACTIVE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(equipment.Status, "OUT_OF_SERVICE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(equipment.Status, "MAINTENANCE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(equipment.Status, "DECOMMISSIONED", StringComparison.OrdinalIgnoreCase))
            return false;

        // Allow compatible existing lifecycle values (operational, low_fuel, critical, active, available)
        var isCompatible = string.Equals(equipment.Status, "OPERATIONAL", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(equipment.Status, "LOW_FUEL", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(equipment.Status, "CRITICAL", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(equipment.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(equipment.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase);
        if (!isCompatible)
            return false;

        // Fuel type compatibility check
        if (!string.IsNullOrWhiteSpace(fuelType) &&
            !string.Equals(equipment.RequiredFuelType, fuelType, StringComparison.OrdinalIgnoreCase))
            return false;

        // Capacity check: equipment cannot accept refill if already at or beyond capacity
        if (equipment.CurrentLevel >= equipment.Capacity)
            return false;

        int refillUnits;
        try
        {
            refillUnits = FuelUnitConverter.ConvertToEquipmentUnits(quantity, unit, equipment.Unit);
        }
        catch (OverflowException)
        {
            return false;
        }

        if (refillUnits <= 0)
            return false;

        try
        {
            equipment.Refill(refillUnits);
            equipmentRepository.Update(equipment);

            var refill = new RefillHistory(new CreateRefillHistoryCommand(
                equipmentId,
                companyId,
                providerId,
                fuelType,
                refillUnits,
                requestId));

            await refillHistoryRepository.AddAsync(refill, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Task<bool> RefillEquipmentAsync(int equipmentId, int companyId, int quantity, string fuelType,
        int? providerId, int? requestId, CancellationToken cancellationToken = default)
    {
        return RefillEquipmentAsync(equipmentId, companyId, quantity, "LITERS", fuelType, providerId, requestId, cancellationToken);
    }
}
