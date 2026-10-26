namespace TechnoSac.FullTank.Platform.Payment.Domain.Model.Queries;

/// <summary>Query to get payments for a given order id.</summary>
public record GetPaymentsByOrderIdQuery(int OrderId);
