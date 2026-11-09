using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.ReadModels;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Interfaces.Rest.Transform;

/// <summary>Maps buyer analytics read models to their REST resources.</summary>
public static class BuyerAnalyticsResourceAssembler
{
    public static BuyerDashboardResource ToResource(BuyerDashboard model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return new BuyerDashboardResource(
            model.ActiveOrders,
            model.PendingPayments,
            model.NeedsRefill,
            model.TotalSpent,
            model.SpendingTrend.Select(ToResource).ToList(),
            model.RecentOrders.Select(ToResource).ToList());
    }

    public static BuyerSpendingSummaryResource ToResource(BuyerSpendingSummary model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return new BuyerSpendingSummaryResource(model.TotalSpent, model.TotalOrders, model.AverageOrderValue);
    }

    public static BuyerMonthlySpendingResource ToResource(MonthlySpendingPoint model)
    {
        return new BuyerMonthlySpendingResource(model.Month, model.Amount);
    }

    public static BuyerSpendingByProviderResource ToResource(SpendingByProvider model)
    {
        return new BuyerSpendingByProviderResource(model.ProviderId, model.ProviderName, model.Amount);
    }

    public static BuyerSpendingByFuelTypeResource ToResource(SpendingByFuelType model)
    {
        return new BuyerSpendingByFuelTypeResource(model.FuelType, model.Amount);
    }

    public static BuyerSpendingByEquipmentResource ToResource(SpendingByEquipment model)
    {
        return new BuyerSpendingByEquipmentResource(model.EquipmentId, model.EquipmentName, model.Amount);
    }

    public static SpendingTrendPointResource ToResource(SpendingTrendPoint model)
    {
        return new SpendingTrendPointResource(model.Label, model.Amount);
    }

    public static RecentOrderResource ToResource(RecentOrder model)
    {
        return new RecentOrderResource(model.OrderId, model.Status, model.Amount, model.Date);
    }
}
