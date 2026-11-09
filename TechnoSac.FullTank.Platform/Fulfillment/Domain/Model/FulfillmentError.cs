namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model;

/// <summary>Domain/application errors for the Fulfillment bounded context.</summary>
public enum FulfillmentError
{
    None,
    NotFound,
    ValidationError,
    OperationCancelled,
    DatabaseError,
    InternalServerError
}
