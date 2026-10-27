using Microsoft.EntityFrameworkCore;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Application.OutboundServices;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.ReadModels;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;

namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Infrastructure.Persistence.EntityFrameworkCore;

public class AnalyticsReadStore(AppDbContext context) : IAnalyticsReadStore
{
    private static bool IsPaid(Order order)
    {
        return order.Status is "PAID" or "CLOSED" || order.PaymentStatus == "PAID";
    }

    private async Task<List<Order>> BuyerOrders(int buyerCompanyId, CancellationToken cancellationToken)
    {
        return await context.Set<Order>().AsNoTracking()
            .Where(order => order.BuyerCompanyId == buyerCompanyId)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<Order>> ProviderOrders(int providerId, CancellationToken cancellationToken)
    {
        return await context.Set<Order>().AsNoTracking()
            .Where(order => order.ProviderId == providerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<BuyerDashboard> GetBuyerDashboard(int buyerCompanyId,
        CancellationToken cancellationToken)
    {
        var orders = await BuyerOrders(buyerCompanyId, cancellationToken);
        var paid = orders.Where(IsPaid).ToList();
        var equipment = await context.Set<EquipmentAggregate>().AsNoTracking()
            .Where(item => item.CompanyId == buyerCompanyId)
            .ToListAsync(cancellationToken);

        var trend = MonthlyTrend(paid, order => order.TotalAmount)
            .Select(point => new SpendingTrendPoint(point.Label, point.Amount))
            .ToList();
        var recent = orders.OrderByDescending(order => order.CreatedAt)
            .Take(5)
            .Select(order => new RecentOrder(order.Id, order.Status, order.TotalAmount,
                order.CreatedAt?.ToString("O")))
            .ToList();

        return new BuyerDashboard(
            orders.Count(order => order.Status is "ACCEPTED" or "DISPATCHED"),
            orders.Count(order => order.Status == "PENDING_PAYMENT"),
            equipment.Count(item => item.Capacity > 0
                                    && item.CurrentLevel * 100m / item.Capacity <= item.RefillThreshold),
            paid.Sum(order => order.TotalAmount),
            trend,
            recent);
    }

    public async Task<BuyerSpendingSummary> GetBuyerSpendingSummary(int buyerCompanyId, int? year, int? month,
        CancellationToken cancellationToken)
    {
        var paid = InPeriod(await BuyerOrders(buyerCompanyId, cancellationToken), year, month).Where(IsPaid).ToList();
        var total = paid.Sum(order => order.TotalAmount);
        return new BuyerSpendingSummary(total, paid.Count, paid.Count == 0 ? 0m : total / paid.Count);
    }

    public async Task<IEnumerable<MonthlySpendingPoint>> GetBuyerMonthlySpending(int buyerCompanyId, int? year,
        int? month,
        CancellationToken cancellationToken)
    {
        var paid = InPeriod(await BuyerOrders(buyerCompanyId, cancellationToken), year, month).Where(IsPaid);
        return MonthlyTrend(paid, order => order.TotalAmount)
            .Select(point => new MonthlySpendingPoint(point.Label, point.Amount));
    }

    public async Task<IEnumerable<SpendingByProvider>> GetBuyerSpendingByProvider(int buyerCompanyId, int? year,
        int? month,
        CancellationToken cancellationToken)
    {
        var paid = InPeriod(await BuyerOrders(buyerCompanyId, cancellationToken), year, month)
            .Where(IsPaid)
            .Where(order => order.ProviderId.HasValue)
            .ToList();
        var providerIds = paid.Select(order => order.ProviderId!.Value).Distinct().ToList();
        var names = await context.Set<ProviderCompany>().AsNoTracking()
            .Where(provider => providerIds.Contains(provider.Id))
            .ToDictionaryAsync(provider => provider.Id, provider => provider.Name, cancellationToken);

        return paid.GroupBy(order => order.ProviderId!.Value)
            .Select(group => new SpendingByProvider(group.Key,
                names.GetValueOrDefault(group.Key, $"#{group.Key}"), group.Sum(order => order.TotalAmount)));
    }

    public async Task<IEnumerable<SpendingByFuelType>> GetBuyerSpendingByFuelType(int buyerCompanyId, int? year,
        int? month,
        CancellationToken cancellationToken)
    {
        return InPeriod(await BuyerOrders(buyerCompanyId, cancellationToken), year, month)
            .Where(IsPaid)
            .GroupBy(order => order.FuelType)
            .Select(group => new SpendingByFuelType(group.Key, group.Sum(order => order.TotalAmount)));
    }

    public async Task<IEnumerable<SpendingByEquipment>> GetBuyerSpendingByEquipment(int buyerCompanyId, int? year,
        int? month,
        CancellationToken cancellationToken)
    {
        var paid = InPeriod(await BuyerOrders(buyerCompanyId, cancellationToken), year, month)
            .Where(IsPaid)
            .Where(order => order.EquipmentId.HasValue)
            .ToList();
        var equipmentIds = paid.Select(order => order.EquipmentId!.Value).Distinct().ToList();
        var names = await context.Set<EquipmentAggregate>().AsNoTracking()
            .Where(item => equipmentIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);

        return paid.GroupBy(order => order.EquipmentId!.Value)
            .Select(group => new SpendingByEquipment(group.Key,
                names.GetValueOrDefault(group.Key, $"#{group.Key}"), group.Sum(order => order.TotalAmount)));
    }

    public async Task<ProviderDashboard> GetProviderDashboard(int providerId,
        CancellationToken cancellationToken)
    {
        var orders = await ProviderOrders(providerId, cancellationToken);
        var paid = orders.Where(IsPaid).ToList();
        var active = orders.Where(order => order.Status is "ACCEPTED" or "DISPATCHED" or "PENDING_PAYMENT")
            .ToList();
        var pendingRequests = await context.Set<Request>().AsNoTracking()
            .CountAsync(request => request.ProviderId == providerId && request.Status == "PENDING",
                cancellationToken);

        return new ProviderDashboard(
            active.Sum(order => order.Quantity),
            active.Count,
            pendingRequests,
            orders.Where(order => order.Status == "PENDING_PAYMENT").Sum(order => order.TotalAmount),
            MonthlyTrend(paid, order => order.TotalAmount)
                .Select(point => new SalesTrendPoint(point.Label, point.Amount)).ToList(),
            active.OrderByDescending(order => order.CreatedAt).Take(5)
                .Select(order => new ActiveOrderPreview(order.Id, order.BuyerCompanyId, order.Status,
                    order.TotalAmount, order.FuelType)).ToList());
    }

    public async Task<ProviderSalesSummary> GetProviderSalesSummary(int providerId, int? year, int? month,
        CancellationToken cancellationToken)
    {
        var paid = InPeriod(await ProviderOrders(providerId, cancellationToken), year, month).Where(IsPaid).ToList();
        var total = paid.Sum(order => order.TotalAmount);
        return new ProviderSalesSummary(total, paid.Count, paid.Count == 0 ? 0m : total / paid.Count);
    }

    public async Task<IEnumerable<SalesTrendPoint>> GetProviderRevenueOverTime(int providerId, int? year, int? month,
        CancellationToken cancellationToken)
    {
        var paid = InPeriod(await ProviderOrders(providerId, cancellationToken), year, month).Where(IsPaid);
        return MonthlyTrend(paid, order => order.TotalAmount)
            .Select(point => new SalesTrendPoint(point.Label, point.Amount));
    }

    public async Task<IEnumerable<RevenueByFuelType>> GetProviderRevenueByFuelType(int providerId, int? year,
        int? month,
        CancellationToken cancellationToken)
    {
        return InPeriod(await ProviderOrders(providerId, cancellationToken), year, month)
            .Where(IsPaid)
            .GroupBy(order => order.FuelType)
            .Select(group => new RevenueByFuelType(group.Key, group.Sum(order => order.TotalAmount)));
    }

    public async Task<IEnumerable<OrdersByStatus>> GetProviderOrdersByStatus(int providerId, int? year, int? month,
        CancellationToken cancellationToken)
    {
        return InPeriod(await ProviderOrders(providerId, cancellationToken), year, month)
            .GroupBy(order => order.Status)
            .Select(group => new OrdersByStatus(group.Key, group.Count()));
    }

    public async Task<IEnumerable<CustomersBySector>> GetProviderCustomersBySector(int providerId, int? year,
        int? month,
        CancellationToken cancellationToken)
    {
        var companyIds = InPeriod(await ProviderOrders(providerId, cancellationToken), year, month)
            .Where(order => order.BuyerCompanyId.HasValue)
            .Select(order => order.BuyerCompanyId!.Value)
            .Distinct()
            .ToList();

        return (await context.Set<BuyerCompany>().AsNoTracking()
                .Where(company => companyIds.Contains(company.Id))
                .ToListAsync(cancellationToken))
            .GroupBy(company => company.Sector)
            .Select(group => new CustomersBySector(group.Key, group.Count()));
    }

    public async Task<IEnumerable<ProviderTopCustomer>> GetProviderTopCustomers(int providerId, int? year,
        int? month,
        CancellationToken cancellationToken)
    {
        var paid = InPeriod(await ProviderOrders(providerId, cancellationToken), year, month)
            .Where(IsPaid)
            .Where(order => order.BuyerCompanyId.HasValue)
            .ToList();
        var companyIds = paid.Select(order => order.BuyerCompanyId!.Value).Distinct().ToList();
        var names = await context.Set<BuyerCompany>().AsNoTracking()
            .Where(company => companyIds.Contains(company.Id))
            .ToDictionaryAsync(company => company.Id, company => company.Name, cancellationToken);

        return paid.GroupBy(order => order.BuyerCompanyId!.Value)
            .Select(group => new ProviderTopCustomer(group.Key,
                names.GetValueOrDefault(group.Key, $"#{group.Key}"),
                group.Sum(order => order.TotalAmount), group.Count()))
            .OrderByDescending(customer => customer.TotalPurchased)
            .Take(10);
    }

    private static IEnumerable<(string Label, decimal Amount)> MonthlyTrend(
        IEnumerable<Order> orders, Func<Order, decimal> amount)
    {
        return orders.Where(order => order.CreatedAt.HasValue)
            .GroupBy(order => new { order.CreatedAt!.Value.Year, order.CreatedAt.Value.Month })
            .OrderBy(group => group.Key.Year)
            .ThenBy(group => group.Key.Month)
            .Select(group => ($"{group.Key.Year:D4}-{group.Key.Month:D2}", group.Sum(amount)));
    }

    private static IEnumerable<Order> InPeriod(IEnumerable<Order> orders, int? year, int? month)
    {
        return orders.Where(order =>
            (!year.HasValue || order.CreatedAt?.Year == year.Value)
            && (!month.HasValue || order.CreatedAt?.Month == month.Value));
    }
}
