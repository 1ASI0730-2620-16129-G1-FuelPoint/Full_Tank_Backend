namespace TechnoSac.FullTank.Platform.Ordering.Domain.Model;

/// <summary>Domain/application errors for the Ordering bounded context.</summary>
public enum OrderingError
{
    None,
    NotFound,
    ValidationError,
    OperationCancelled,
    DatabaseError,
    InternalServerError
}
