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
[Route("api/v1/invoices")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Invoice endpoints")]
public class InvoicesController(
    IInvoiceCommandService commandService,
    IInvoiceQueryService queryService,
    IPaymentQueryService paymentQueryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get all invoices", OperationId = "GetAllInvoices")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<InvoiceResource>))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await queryService.Handle(new GetAllInvoicesQuery(), cancellationToken);
        var payments = await paymentQueryService.Handle(
            new GetAllPaymentsQuery(this.CurrentUser().CompanyId), cancellationToken);
        var paymentIds = payments.Select(payment => payment.Id).ToHashSet();
        return Ok(items.Where(invoice => invoice.PaymentId.HasValue && paymentIds.Contains(invoice.PaymentId.Value))
            .Select(InvoiceResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get an invoice by id", OperationId = "GetInvoiceById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(InvoiceResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await queryService.Handle(new GetInvoiceByIdQuery(id), cancellationToken);
        if (item?.PaymentId is int paymentId
            && !await OwnsPayment(paymentId, cancellationToken))
            return this.Forbidden();
        return PaymentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(InvoiceResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpGet("payment/{paymentId:int}")]
    [SwaggerOperation(Summary = "Get an invoice by payment id", OperationId = "GetInvoiceByPayment")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(InvoiceResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetByPayment(int paymentId, CancellationToken cancellationToken)
    {
        if (!await OwnsPayment(paymentId, cancellationToken)) return this.Forbidden();
        var item = await queryService.Handle(new GetInvoiceByPaymentIdQuery(paymentId), cancellationToken);
        return PaymentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(InvoiceResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpGet("order/{orderId:int}")]
    [SwaggerOperation(Summary = "Get an invoice by order id", OperationId = "GetInvoiceByOrder")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(InvoiceResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    public async Task<IActionResult> GetByOrder(int orderId, CancellationToken cancellationToken)
    {
        var payments = await paymentQueryService.Handle(new GetPaymentsByOrderIdQuery(orderId), cancellationToken);
        if (!payments.Any(payment => this.OwnsBuyerCompany(payment.CompanyId))) return this.Forbidden();
        var item = await queryService.Handle(new GetInvoiceByOrderIdQuery(orderId), cancellationToken);
        return PaymentActionResultAssembler.ToActionResultFromEntity(this, item, errorLocalizer,
            problemDetailsFactory, found => Ok(InvoiceResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [Authorize("ADMIN")]
    [SwaggerOperation(Summary = "Create an invoice (disabled)", OperationId = "CreateInvoice")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Direct invoice creation is disabled")]
    public IActionResult Create([FromBody] CreateInvoiceResource resource,
        CancellationToken cancellationToken)
    {
        return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status400BadRequest,
            PaymentError.ValidationError,
            "Direct invoice creation is disabled. Use POST /api/v1/payment-checkout.");
    }

    [HttpPut("{id:int}")]
    [Authorize("ADMIN")]
    [SwaggerOperation(Summary = "Update an invoice (disabled)", OperationId = "UpdateInvoice")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Direct invoice modification is disabled")]
    public IActionResult Update(int id, [FromBody] UpdateInvoiceResource resource,
        CancellationToken cancellationToken)
    {
        return problemDetailsFactory.CreateProblemDetails(this, StatusCodes.Status400BadRequest,
            PaymentError.ValidationError,
            "Direct invoice modification is disabled.");
    }

    private async Task<bool> OwnsPayment(int paymentId, CancellationToken cancellationToken)
    {
        var payment = await paymentQueryService.Handle(new GetPaymentByIdQuery(paymentId), cancellationToken);
        return payment is not null && this.OwnsBuyerCompany(payment.CompanyId);
    }
}
