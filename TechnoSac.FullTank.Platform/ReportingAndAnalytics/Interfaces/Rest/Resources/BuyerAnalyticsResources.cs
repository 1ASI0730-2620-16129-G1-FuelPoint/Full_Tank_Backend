namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Interfaces.Rest.Resources;

// Buyer analytics API resources. Serialized as camelCase JSON by the global serializer configuration.

public record SpendingTrendPointResource(string Label, decimal Amount);

public record RecentOrderResource(int OrderId, string Status, decimal Amount, string? Date);

public record BuyerDashboardResource(
    int ActiveOrders,
    int PendingPayments,
    int NeedsRefill,
    decimal TotalSpent,
    IReadOnlyList<SpendingTrendPointResource> SpendingTrend,
    IReadOnlyList<RecentOrderResource> RecentOrders);

public record BuyerSpendingSummaryResource(decimal TotalSpent, int TotalOrders, decimal AverageOrderValue);

public record BuyerMonthlySpendingResource(string Month, decimal Amount);

public record BuyerSpendingByProviderResource(int ProviderId, string ProviderName, decimal Amount);

public record BuyerSpendingByFuelTypeResource(string FuelType, decimal Amount);

public record BuyerSpendingByEquipmentResource(int EquipmentId, string EquipmentName, decimal Amount);
