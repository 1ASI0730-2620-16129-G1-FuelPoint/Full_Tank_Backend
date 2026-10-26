namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.ReadModels;

// Provider-facing read models for dashboards and sales reports. Non-persisted, computed/derived only.

/// <summary>Aggregated provider dashboard snapshot.</summary>
public record ProviderDashboard(
    decimal FuelInActiveOrders,
    int ActiveOrders,
    int PendingRequests,
    decimal ToCollect,
    IReadOnlyList<SalesTrendPoint> SalesTrend,
    IReadOnlyList<ActiveOrderPreview> ActiveOrdersPreview)
{
    public static ProviderDashboard Empty()
    {
        return new ProviderDashboard(0m, 0, 0, 0m, [], []);
    }
}

/// <summary>Headline provider sales figures.</summary>
public record ProviderSalesSummary(decimal TotalRevenue, int TotalOrders, decimal AverageOrderValue)
{
    public static ProviderSalesSummary Empty()
    {
        return new ProviderSalesSummary(0m, 0, 0m);
    }
}

/// <summary>Provider revenue grouped by fuel type.</summary>
public record RevenueByFuelType(string FuelType, decimal Revenue);

/// <summary>Provider order count grouped by status.</summary>
public record OrdersByStatus(string Status, int Count);

/// <summary>Provider customer count grouped by business sector.</summary>
public record CustomersBySector(string Sector, int Count);

/// <summary>A top customer of a provider, ranked by total purchased.</summary>
public record ProviderTopCustomer(int CompanyId, string CompanyName, decimal TotalPurchased, int OrderCount);
