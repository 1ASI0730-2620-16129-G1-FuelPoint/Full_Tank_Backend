using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Application.QueryServices;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Application.OutboundServices;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.ReadModels;

namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Application.Internal.QueryServices;

/// <summary>Read-only analytics application service backed by an outbound reporting projection port.</summary>
public class AnalyticsQueryService(IAnalyticsReadStore readStore) : IAnalyticsQueryService
{
    // Buyer analytics.

    public Task<BuyerDashboard> Handle(GetBuyerDashboardQuery query, CancellationToken cancellationToken)
    {
        return readStore.GetBuyerDashboard(query.BuyerCompanyId, cancellationToken);
    }

    public Task<BuyerSpendingSummary> Handle(GetBuyerSpendingSummaryQuery query, CancellationToken cancellationToken)
    {
        return readStore.GetBuyerSpendingSummary(query.BuyerCompanyId, query.Year, query.Month, cancellationToken);
    }

    public Task<IEnumerable<MonthlySpendingPoint>> Handle(GetBuyerMonthlySpendingQuery query,
        CancellationToken cancellationToken)
    {
        return readStore.GetBuyerMonthlySpending(query.BuyerCompanyId, query.Year, query.Month, cancellationToken);
    }

    public Task<IEnumerable<SpendingByProvider>> Handle(GetBuyerSpendingByProviderQuery query,
        CancellationToken cancellationToken)
    {
        return readStore.GetBuyerSpendingByProvider(query.BuyerCompanyId, query.Year, query.Month, cancellationToken);
    }

    public Task<IEnumerable<SpendingByFuelType>> Handle(GetBuyerSpendingByFuelTypeQuery query,
        CancellationToken cancellationToken)
    {
        return readStore.GetBuyerSpendingByFuelType(query.BuyerCompanyId, query.Year, query.Month, cancellationToken);
    }

    public Task<IEnumerable<SpendingByEquipment>> Handle(GetBuyerSpendingByEquipmentQuery query,
        CancellationToken cancellationToken)
    {
        return readStore.GetBuyerSpendingByEquipment(query.BuyerCompanyId, query.Year, query.Month, cancellationToken);
    }

    // Provider analytics.

    public Task<ProviderDashboard> Handle(GetProviderDashboardQuery query, CancellationToken cancellationToken)
    {
        return readStore.GetProviderDashboard(query.ProviderId, cancellationToken);
    }

    public Task<ProviderSalesSummary> Handle(GetProviderSalesSummaryQuery query, CancellationToken cancellationToken)
    {
        return readStore.GetProviderSalesSummary(query.ProviderId, query.Year, query.Month, cancellationToken);
    }

    public Task<IEnumerable<SalesTrendPoint>> Handle(GetProviderRevenueOverTimeQuery query,
        CancellationToken cancellationToken)
    {
        return readStore.GetProviderRevenueOverTime(query.ProviderId, query.Year, query.Month, cancellationToken);
    }

    public Task<IEnumerable<RevenueByFuelType>> Handle(GetProviderRevenueByFuelTypeQuery query,
        CancellationToken cancellationToken)
    {
        return readStore.GetProviderRevenueByFuelType(query.ProviderId, query.Year, query.Month, cancellationToken);
    }

    public Task<IEnumerable<OrdersByStatus>> Handle(GetProviderOrdersByStatusQuery query,
        CancellationToken cancellationToken)
    {
        return readStore.GetProviderOrdersByStatus(query.ProviderId, query.Year, query.Month, cancellationToken);
    }

    public Task<IEnumerable<CustomersBySector>> Handle(GetProviderCustomersBySectorQuery query,
        CancellationToken cancellationToken)
    {
        return readStore.GetProviderCustomersBySector(query.ProviderId, query.Year, query.Month, cancellationToken);
    }

    public Task<IEnumerable<ProviderTopCustomer>> Handle(GetProviderTopCustomersQuery query,
        CancellationToken cancellationToken)
    {
        return readStore.GetProviderTopCustomers(query.ProviderId, query.Year, query.Month, cancellationToken);
    }
}
