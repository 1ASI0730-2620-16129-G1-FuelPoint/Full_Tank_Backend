namespace TechnoSac.FullTank.Platform.Payment.Domain.Model;

/// <summary>Domain/application errors for the Payment bounded context.</summary>
public enum PaymentError
{
    None,
    NotFound,
    ValidationError,
    OperationCancelled,
    DatabaseError,
    InternalServerError
}
