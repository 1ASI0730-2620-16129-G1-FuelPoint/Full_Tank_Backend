using TechnoSac.FullTank.Platform.Payment.Domain.Model;
using System.Net.Mime;
using TechnoSac.FullTank.Platform.Payment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Payment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;

namespace TechnoSac.FullTank.Platform.Payment.Interfaces.Rest;

[ApiController]
[Authorize("BUYER")]
[Route("api/v1/payments")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Payment endpoints")]
public class PaymentsController(
    IPaymentCommandService commandService,
    IPaymentQueryService queryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get payments (optionally by companyId)", OperationId = "GetAllPayments")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<PaymentResource>))]
    public async Task<IActionResult> GetAll([FromQuery] int? companyId, CancellationToken cancellationToken)
    {
        companyId = this.CurrentUser().CompanyId;
        var items = await queryService.Handle(new GetAllPaymentsQuery(companyId), cancellationToken);
        return Ok(items.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a payment by id", OperationId = "GetPaymentById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(PaymentResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetPaymentByIdQuery(id), cancellationToken);
        if (item is not null && !this.OwnsBuyerCompany(item.CompanyId)) return this.Forbidden();
        return PaymentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(PaymentResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpGet("company/{companyId:int}")]
    [SwaggerOperation(Summary = "Get payments by buyer company id", OperationId = "GetPaymentsByCompany")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<PaymentResource>))]
    public async Task<IActionResult> GetByCompany(int companyId, CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(companyId)) return this.Forbidden();
        var items = await queryService.Handle(new GetAllPaymentsQuery(companyId), cancellationToken);
        return Ok(items.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("order/{orderId:int}")]
    [SwaggerOperation(Summary = "Get payments by order id", OperationId = "GetPaymentsByOrder")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<PaymentResource>))]
    public async Task<IActionResult> GetByOrder(int orderId, CancellationToken cancellationToken)
    {
        var items = await queryService.Handle(new GetPaymentsByOrderIdQuery(orderId), cancellationToken);
        if (items.Any(item => !this.OwnsBuyerCompany(item.CompanyId))) return this.Forbidden();
        return Ok(items.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [Authorize("ADMIN")]
    [SwaggerOperation(Summary = "Create a payment (disabled)", OperationId = "CreatePayment")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Direct payment creation is disabled")]
    public IActionResult Create([FromBody] CreatePaymentResource resource,
        CancellationToken cancellationToken)
    {
        return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status400BadRequest,
            PaymentError.ValidationError,
            "Direct payment creation is disabled. Use POST /api/v1/payment-checkout.");
    }

    [HttpPut("{id:int}")]
    [Authorize("ADMIN")]
    [SwaggerOperation(Summary = "Update a payment (disabled)", OperationId = "UpdatePayment")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Direct payment modification is disabled")]
    public IActionResult Update(int id, [FromBody] UpdatePaymentResource resource,
        CancellationToken cancellationToken)
    {
        return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status400BadRequest,
            PaymentError.ValidationError,
            "Direct payment modification is disabled.");
    }
}
