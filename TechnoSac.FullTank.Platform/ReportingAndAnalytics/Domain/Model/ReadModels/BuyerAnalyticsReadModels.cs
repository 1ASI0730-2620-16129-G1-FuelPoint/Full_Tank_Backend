namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.ReadModels;

// Buyer-facing read models for dashboards and spending reports. Non-persisted, computed/derived only.

/// <summary>Aggregated buyer dashboard snapshot.</summary>
public record BuyerDashboard(
    int ActiveOrders,
    int PendingPayments,
    int NeedsRefill,
    decimal TotalSpent,
    IReadOnlyList<SpendingTrendPoint> SpendingTrend,
    IReadOnlyList<RecentOrder> RecentOrders)
{
    public static BuyerDashboard Empty()
    {
        return new BuyerDashboard(0, 0, 0, 0m, [], []);
    }
}

/// <summary>Headline buyer spending figures.</summary>
public record BuyerSpendingSummary(decimal TotalSpent, int TotalOrders, decimal AverageOrderValue)
{
    public static BuyerSpendingSummary Empty()
    {
        return new BuyerSpendingSummary(0m, 0, 0m);
    }
}

/// <summary>Buyer spending grouped by provider.</summary>
public record SpendingByProvider(int ProviderId, string ProviderName, decimal Amount);

/// <summary>Buyer spending grouped by fuel type.</summary>
public record SpendingByFuelType(string FuelType, decimal Amount);

/// <summary>Buyer spending grouped by equipment.</summary>
public record SpendingByEquipment(int EquipmentId, string EquipmentName, decimal Amount);
