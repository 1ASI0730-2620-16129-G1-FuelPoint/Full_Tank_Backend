using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.ReadModels;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Interfaces.Rest.Transform;

/// <summary>Maps provider analytics read models to their REST resources.</summary>
public static class ProviderAnalyticsResourceAssembler
{
    public static ProviderDashboardResource ToResource(ProviderDashboard model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return new ProviderDashboardResource(
            model.FuelInActiveOrders,
            model.ActiveOrders,
            model.PendingRequests,
            model.ToCollect,
            model.SalesTrend.Select(ToResource).ToList(),
            model.ActiveOrdersPreview.Select(ToResource).ToList());
    }

    public static ProviderSalesSummaryResource ToResource(ProviderSalesSummary model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return new ProviderSalesSummaryResource(model.TotalRevenue, model.TotalOrders, model.AverageOrderValue);
    }

    public static ProviderRevenueOverTimeResource ToRevenueOverTimeResource(SalesTrendPoint model)
    {
        return new ProviderRevenueOverTimeResource(model.Label, model.Amount);
    }

    public static ProviderRevenueByFuelTypeResource ToResource(RevenueByFuelType model)
    {
        return new ProviderRevenueByFuelTypeResource(model.FuelType, model.Revenue);
    }

    public static ProviderOrdersByStatusResource ToResource(OrdersByStatus model)
    {
        return new ProviderOrdersByStatusResource(model.Status, model.Count);
    }

    public static ProviderCustomersBySectorResource ToResource(CustomersBySector model)
    {
        return new ProviderCustomersBySectorResource(model.Sector, model.Count);
    }

    public static ProviderTopCustomerResource ToResource(ProviderTopCustomer model)
    {
        return new ProviderTopCustomerResource(model.CompanyId, model.CompanyName, model.TotalPurchased,
            model.OrderCount);
    }

    public static SalesTrendPointResource ToResource(SalesTrendPoint model)
    {
        return new SalesTrendPointResource(model.Label, model.Amount);
    }

    public static ActiveOrderPreviewResource ToResource(ActiveOrderPreview model)
    {
        return new ActiveOrderPreviewResource(model.OrderId, model.BuyerCompanyId, model.Status, model.Amount,
            model.FuelType);
    }
}
