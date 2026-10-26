namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.Queries;

public record GetProviderDashboardQuery(int ProviderId);
public record GetProviderSalesSummaryQuery(int ProviderId, int? Year = null, int? Month = null);
public record GetProviderRevenueOverTimeQuery(int ProviderId, int? Year = null, int? Month = null);
public record GetProviderRevenueByFuelTypeQuery(int ProviderId, int? Year = null, int? Month = null);
public record GetProviderOrdersByStatusQuery(int ProviderId, int? Year = null, int? Month = null);
public record GetProviderCustomersBySectorQuery(int ProviderId, int? Year = null, int? Month = null);
public record GetProviderTopCustomersQuery(int ProviderId, int? Year = null, int? Month = null);
