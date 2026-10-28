namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.ReadModels;

// Shared, non-persisted read-model points used to compose dashboards and reports.
// These types belong to the ReportingAndAnalytics bounded context only; they never reference
// aggregates/entities from other bounded contexts (they carry plain ids and derived values).

/// <summary>A single point of a buyer spending trend (e.g. one bucket of a time series).</summary>
public record SpendingTrendPoint(string Label, decimal Amount);

/// <summary>A single point of a provider sales/revenue trend.</summary>
public record SalesTrendPoint(string Label, decimal Amount);

/// <summary>Buyer spending grouped by calendar month.</summary>
public record MonthlySpendingPoint(string Month, decimal Amount);

/// <summary>A compact preview of a recent buyer order (ids and derived values only).</summary>
public record RecentOrder(int OrderId, string Status, decimal Amount, string? Date);

/// <summary>A compact preview of an active provider order (ids and derived values only).</summary>
public record ActiveOrderPreview(int OrderId, int? BuyerCompanyId, string Status, decimal Amount, string FuelType);
