namespace TechnoSac.FullTank.Platform.Catalog.Domain.Model;

/// <summary>Domain/application errors for the Catalog bounded context.</summary>
public enum CatalogError
{
    None,
    NotFound,
    ValidationError,
    OperationCancelled,
    DatabaseError,
    InternalServerError
}
