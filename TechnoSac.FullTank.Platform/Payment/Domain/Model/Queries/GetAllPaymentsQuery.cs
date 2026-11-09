namespace TechnoSac.FullTank.Platform.Payment.Domain.Model.Queries;

/// <summary>Query to get payments, optionally filtered by buyer company id.</summary>
public record GetAllPaymentsQuery(int? CompanyId = null);
