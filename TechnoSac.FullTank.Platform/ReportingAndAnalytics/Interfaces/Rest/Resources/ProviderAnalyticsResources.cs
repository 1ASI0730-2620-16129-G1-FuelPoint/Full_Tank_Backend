namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Interfaces.Rest.Resources;

// Provider analytics API resources. Serialized as camelCase JSON by the global serializer configuration.

public record SalesTrendPointResource(string Label, decimal Amount);

public record ActiveOrderPreviewResource(
    int OrderId,
    int? BuyerCompanyId,
    string Status,
    decimal Amount,
    string FuelType);

public record ProviderDashboardResource(
    decimal FuelInActiveOrders,
    int ActiveOrders,
    int PendingRequests,
    decimal ToCollect,
    IReadOnlyList<SalesTrendPointResource> SalesTrend,
    IReadOnlyList<ActiveOrderPreviewResource> ActiveOrdersPreview);

public record ProviderSalesSummaryResource(decimal TotalRevenue, int TotalOrders, decimal AverageOrderValue);

public record ProviderRevenueOverTimeResource(string Label, decimal Amount);

public record ProviderRevenueByFuelTypeResource(string FuelType, decimal Revenue);

public record ProviderOrdersByStatusResource(string Status, int Count);

public record ProviderCustomersBySectorResource(string Sector, int Count);

public record ProviderTopCustomerResource(int CompanyId, string CompanyName, decimal TotalPurchased, int OrderCount);
