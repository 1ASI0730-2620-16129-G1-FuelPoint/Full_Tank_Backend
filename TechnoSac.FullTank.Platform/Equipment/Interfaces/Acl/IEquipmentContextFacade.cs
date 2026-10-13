namespace TechnoSac.FullTank.Platform.Equipment.Interfaces.Acl;

public interface IEquipmentContextFacade
{
    Task<bool> RefillEquipmentAsync(int equipmentId, int companyId, int quantity, string unit, string fuelType, int? providerId,
        int? requestId, CancellationToken cancellationToken = default);

    Task<bool> RefillEquipmentAsync(int equipmentId, int companyId, int quantity, string fuelType, int? providerId,
        int? requestId, CancellationToken cancellationToken = default);
}

