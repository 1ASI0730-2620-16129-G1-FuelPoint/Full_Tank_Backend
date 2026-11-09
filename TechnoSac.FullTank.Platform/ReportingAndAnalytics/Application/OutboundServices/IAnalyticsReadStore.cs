using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.ReadModels;

namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Application.OutboundServices;

public interface IAnalyticsReadStore
{
    Task<BuyerDashboard> GetBuyerDashboard(int buyerCompanyId, CancellationToken cancellationToken);
    Task<BuyerSpendingSummary> GetBuyerSpendingSummary(int buyerCompanyId, int? year, int? month,
        CancellationToken cancellationToken);
    Task<IEnumerable<MonthlySpendingPoint>> GetBuyerMonthlySpending(int buyerCompanyId, int? year, int? month,
        CancellationToken cancellationToken);
    Task<IEnumerable<SpendingByProvider>> GetBuyerSpendingByProvider(int buyerCompanyId, int? year, int? month,
        CancellationToken cancellationToken);
    Task<IEnumerable<SpendingByFuelType>> GetBuyerSpendingByFuelType(int buyerCompanyId, int? year, int? month,
        CancellationToken cancellationToken);
    Task<IEnumerable<SpendingByEquipment>> GetBuyerSpendingByEquipment(int buyerCompanyId, int? year, int? month,
        CancellationToken cancellationToken);
    Task<ProviderDashboard> GetProviderDashboard(int providerId, CancellationToken cancellationToken);
    Task<ProviderSalesSummary> GetProviderSalesSummary(int providerId, int? year, int? month,
        CancellationToken cancellationToken);
    Task<IEnumerable<SalesTrendPoint>> GetProviderRevenueOverTime(int providerId, int? year, int? month,
        CancellationToken cancellationToken);
    Task<IEnumerable<RevenueByFuelType>> GetProviderRevenueByFuelType(int providerId, int? year, int? month,
        CancellationToken cancellationToken);
    Task<IEnumerable<OrdersByStatus>> GetProviderOrdersByStatus(int providerId, int? year, int? month,
        CancellationToken cancellationToken);
    Task<IEnumerable<CustomersBySector>> GetProviderCustomersBySector(int providerId, int? year, int? month,
        CancellationToken cancellationToken);
    Task<IEnumerable<ProviderTopCustomer>> GetProviderTopCustomers(int providerId, int? year, int? month,
        CancellationToken cancellationToken);
}
