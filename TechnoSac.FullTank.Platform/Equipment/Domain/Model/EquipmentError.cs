namespace TechnoSac.FullTank.Platform.Equipment.Domain.Model;

/// <summary>Domain/application errors for the Equipment bounded context.</summary>
public enum EquipmentError
{
    None,
    NotFound,
    ValidationError,
    OperationCancelled,
    DatabaseError,
    InternalServerError
}
