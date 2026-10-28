using System.Net.Mime;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Application.QueryServices;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.ReportingAndAnalytics.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;

namespace TechnoSac.FullTank.Platform.ReportingAndAnalytics.Interfaces.Rest;

[ApiController]
[Route("api/v1/analytics")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Reporting and analytics (read-only) endpoints, including buyer and provider dashboards")]
[Authorize("BUYER", "PROVIDER")]
public class AnalyticsController(IAnalyticsQueryService queryService) : ControllerBase
{
    // ----- Buyer analytics -----

    [HttpGet("buyer-dashboard/{buyerCompanyId:int}")]
    [SwaggerOperation(Summary = "Get the buyer dashboard", OperationId = "GetBuyerDashboard")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(BuyerDashboardResource))]
    public async Task<IActionResult> GetBuyerDashboard(int buyerCompanyId, CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(buyerCompanyId)) return this.Forbidden();
        var model = await queryService.Handle(new GetBuyerDashboardQuery(buyerCompanyId), cancellationToken);
        return Ok(BuyerAnalyticsResourceAssembler.ToResource(model));
    }

    [HttpGet("buyer/{buyerCompanyId:int}/spending-summary")]
    [SwaggerOperation(Summary = "Get the buyer spending summary", OperationId = "GetBuyerSpendingSummary")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(BuyerSpendingSummaryResource))]
    public async Task<IActionResult> GetBuyerSpendingSummary(int buyerCompanyId, [FromQuery] int? year,
        [FromQuery] int? month, CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(buyerCompanyId)) return this.Forbidden();
        var model = await queryService.Handle(new GetBuyerSpendingSummaryQuery(buyerCompanyId, year, month),
            cancellationToken);
        return Ok(BuyerAnalyticsResourceAssembler.ToResource(model));
    }

    [HttpGet("buyer/{buyerCompanyId:int}/monthly-spending")]
    [SwaggerOperation(Summary = "Get the buyer monthly spending", OperationId = "GetBuyerMonthlySpending")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<BuyerMonthlySpendingResource>))]
    public async Task<IActionResult> GetBuyerMonthlySpending(int buyerCompanyId, [FromQuery] int? year,
        [FromQuery] int? month, CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(buyerCompanyId)) return this.Forbidden();
        var items = await queryService.Handle(new GetBuyerMonthlySpendingQuery(buyerCompanyId, year, month),
            cancellationToken);
        return Ok(items.Select(BuyerAnalyticsResourceAssembler.ToResource));
    }

    [HttpGet("buyer/{buyerCompanyId:int}/spending-by-provider")]
    [SwaggerOperation(Summary = "Get the buyer spending by provider", OperationId = "GetBuyerSpendingByProvider")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<BuyerSpendingByProviderResource>))]
    public async Task<IActionResult> GetBuyerSpendingByProvider(int buyerCompanyId, [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(buyerCompanyId)) return this.Forbidden();
        var items = await queryService.Handle(new GetBuyerSpendingByProviderQuery(buyerCompanyId, year, month),
            cancellationToken);
        return Ok(items.Select(BuyerAnalyticsResourceAssembler.ToResource));
    }

    [HttpGet("buyer/{buyerCompanyId:int}/spending-by-fuel-type")]
    [SwaggerOperation(Summary = "Get the buyer spending by fuel type", OperationId = "GetBuyerSpendingByFuelType")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<BuyerSpendingByFuelTypeResource>))]
    public async Task<IActionResult> GetBuyerSpendingByFuelType(int buyerCompanyId, [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(buyerCompanyId)) return this.Forbidden();
        var items = await queryService.Handle(new GetBuyerSpendingByFuelTypeQuery(buyerCompanyId, year, month),
            cancellationToken);
        return Ok(items.Select(BuyerAnalyticsResourceAssembler.ToResource));
    }

    [HttpGet("buyer/{buyerCompanyId:int}/spending-by-equipment")]
    [SwaggerOperation(Summary = "Get the buyer spending by equipment", OperationId = "GetBuyerSpendingByEquipment")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<BuyerSpendingByEquipmentResource>))]
    public async Task<IActionResult> GetBuyerSpendingByEquipment(int buyerCompanyId, [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(buyerCompanyId)) return this.Forbidden();
        var items = await queryService.Handle(new GetBuyerSpendingByEquipmentQuery(buyerCompanyId, year, month),
            cancellationToken);
        return Ok(items.Select(BuyerAnalyticsResourceAssembler.ToResource));
    }

    // ----- Provider analytics -----

    [HttpGet("provider-dashboard/{providerId:int}")]
    [SwaggerOperation(Summary = "Get the provider dashboard", OperationId = "GetProviderDashboard")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ProviderDashboardResource))]
    public async Task<IActionResult> GetProviderDashboard(int providerId, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var model = await queryService.Handle(new GetProviderDashboardQuery(providerId), cancellationToken);
        return Ok(ProviderAnalyticsResourceAssembler.ToResource(model));
    }

    [HttpGet("provider/{providerId:int}/sales-summary")]
    [SwaggerOperation(Summary = "Get the provider sales summary", OperationId = "GetProviderSalesSummary")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ProviderSalesSummaryResource))]
    public async Task<IActionResult> GetProviderSalesSummary(int providerId, [FromQuery] int? year,
        [FromQuery] int? month, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var model = await queryService.Handle(new GetProviderSalesSummaryQuery(providerId, year, month),
            cancellationToken);
        return Ok(ProviderAnalyticsResourceAssembler.ToResource(model));
    }

    [HttpGet("provider/{providerId:int}/revenue-over-time")]
    [SwaggerOperation(Summary = "Get the provider revenue over time", OperationId = "GetProviderRevenueOverTime")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<ProviderRevenueOverTimeResource>))]
    public async Task<IActionResult> GetProviderRevenueOverTime(int providerId, [FromQuery] int? year,
        [FromQuery] int? month, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var items = await queryService.Handle(new GetProviderRevenueOverTimeQuery(providerId, year, month),
            cancellationToken);
        return Ok(items.Select(ProviderAnalyticsResourceAssembler.ToRevenueOverTimeResource));
    }

    [HttpGet("provider/{providerId:int}/revenue-by-fuel-type")]
    [SwaggerOperation(Summary = "Get the provider revenue by fuel type", OperationId = "GetProviderRevenueByFuelType")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<ProviderRevenueByFuelTypeResource>))]
    public async Task<IActionResult> GetProviderRevenueByFuelType(int providerId, [FromQuery] int? year,
        [FromQuery] int? month, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var items = await queryService.Handle(new GetProviderRevenueByFuelTypeQuery(providerId, year, month),
            cancellationToken);
        return Ok(items.Select(ProviderAnalyticsResourceAssembler.ToResource));
    }

    [HttpGet("provider/{providerId:int}/orders-by-status")]
    [SwaggerOperation(Summary = "Get the provider orders by status", OperationId = "GetProviderOrdersByStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<ProviderOrdersByStatusResource>))]
    public async Task<IActionResult> GetProviderOrdersByStatus(int providerId, [FromQuery] int? year,
        [FromQuery] int? month, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var items = await queryService.Handle(new GetProviderOrdersByStatusQuery(providerId, year, month),
            cancellationToken);
        return Ok(items.Select(ProviderAnalyticsResourceAssembler.ToResource));
    }

    [HttpGet("provider/{providerId:int}/customers-by-sector")]
    [SwaggerOperation(Summary = "Get the provider customers by sector", OperationId = "GetProviderCustomersBySector")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<ProviderCustomersBySectorResource>))]
    public async Task<IActionResult> GetProviderCustomersBySector(int providerId, [FromQuery] int? year,
        [FromQuery] int? month, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var items = await queryService.Handle(new GetProviderCustomersBySectorQuery(providerId, year, month),
            cancellationToken);
        return Ok(items.Select(ProviderAnalyticsResourceAssembler.ToResource));
    }

    [HttpGet("provider/{providerId:int}/top-customers")]
    [SwaggerOperation(Summary = "Get the provider top customers", OperationId = "GetProviderTopCustomers")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<ProviderTopCustomerResource>))]
    public async Task<IActionResult> GetProviderTopCustomers(int providerId, [FromQuery] int? year,
        [FromQuery] int? month, CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(providerId)) return this.Forbidden();
        var items = await queryService.Handle(new GetProviderTopCustomersQuery(providerId, year, month),
            cancellationToken);
        return Ok(items.Select(ProviderAnalyticsResourceAssembler.ToResource));
    }
}
