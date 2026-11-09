namespace TechnoSac.FullTank.Platform.Inventory.Domain.Model;

/// <summary>Domain/application errors for the Inventory bounded context.</summary>
public enum InventoryError
{
    None,
    NotFound,
    ValidationError,
    OperationCancelled,
    DatabaseError,
    InternalServerError
}
