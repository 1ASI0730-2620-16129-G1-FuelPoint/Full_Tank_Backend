using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.ReadModels;

namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Application.QueryServices;

/// <summary>
///     Read-only analytics query service for the ReportingAndAnalytics bounded context.
///     Returns own read models; it never reads or modifies data owned by other bounded contexts.
/// </summary>
public interface IAnalyticsQueryService
{
    // Buyer analytics.
    Task<BuyerDashboard> Handle(GetBuyerDashboardQuery query, CancellationToken cancellationToken);
    Task<BuyerSpendingSummary> Handle(GetBuyerSpendingSummaryQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<MonthlySpendingPoint>> Handle(GetBuyerMonthlySpendingQuery query,
        CancellationToken cancellationToken);
    Task<IEnumerable<SpendingByProvider>> Handle(GetBuyerSpendingByProviderQuery query,
        CancellationToken cancellationToken);
    Task<IEnumerable<SpendingByFuelType>> Handle(GetBuyerSpendingByFuelTypeQuery query,
        CancellationToken cancellationToken);
    Task<IEnumerable<SpendingByEquipment>> Handle(GetBuyerSpendingByEquipmentQuery query,
        CancellationToken cancellationToken);

    // Provider analytics.
    Task<ProviderDashboard> Handle(GetProviderDashboardQuery query, CancellationToken cancellationToken);
    Task<ProviderSalesSummary> Handle(GetProviderSalesSummaryQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<SalesTrendPoint>> Handle(GetProviderRevenueOverTimeQuery query,
        CancellationToken cancellationToken);
    Task<IEnumerable<RevenueByFuelType>> Handle(GetProviderRevenueByFuelTypeQuery query,
        CancellationToken cancellationToken);
    Task<IEnumerable<OrdersByStatus>> Handle(GetProviderOrdersByStatusQuery query,
        CancellationToken cancellationToken);
    Task<IEnumerable<CustomersBySector>> Handle(GetProviderCustomersBySectorQuery query,
        CancellationToken cancellationToken);
    Task<IEnumerable<ProviderTopCustomer>> Handle(GetProviderTopCustomersQuery query,
        CancellationToken cancellationToken);
}
