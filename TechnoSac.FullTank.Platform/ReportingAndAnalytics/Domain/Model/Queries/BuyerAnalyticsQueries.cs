namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.Queries;

public record GetBuyerDashboardQuery(int BuyerCompanyId);
public record GetBuyerSpendingSummaryQuery(int BuyerCompanyId, int? Year = null, int? Month = null);
public record GetBuyerMonthlySpendingQuery(int BuyerCompanyId, int? Year = null, int? Month = null);
public record GetBuyerSpendingByProviderQuery(int BuyerCompanyId, int? Year = null, int? Month = null);
public record GetBuyerSpendingByFuelTypeQuery(int BuyerCompanyId, int? Year = null, int? Month = null);
public record GetBuyerSpendingByEquipmentQuery(int BuyerCompanyId, int? Year = null, int? Month = null);
