using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TechnoSac.FullTank.Platform.Payment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Payment.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;

namespace TechnoSac.FullTank.Platform.Payment.Interfaces.Rest;

[ApiController]
[Route("api/v1/payment-checkout")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Atomic payment checkout endpoint")]
[Authorize("BUYER")]
public class PaymentCheckoutController(
    IPaymentCheckoutService checkoutService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] CheckoutPaymentResource resource,
        CancellationToken cancellationToken)
    {
        var actorCompanyId = this.CurrentUser().CompanyId;
        if (!this.OwnsBuyerCompany(resource.Payment.CompanyId) || actorCompanyId != resource.Payment.CompanyId)
            return this.Forbidden();
        var payment = CreatePaymentCommandFromResourceAssembler.ToCommandFromResource(resource.Payment);
        var invoice = CreateInvoiceCommandFromResourceAssembler.ToCommandFromResource(resource.Invoice);
        var result = await checkoutService.Handle(new CheckoutPaymentCommand(payment, invoice, actorCompanyId), cancellationToken);
        return PaymentActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            checkout => Ok(new PaymentCheckoutResource(
                PaymentResourceFromEntityAssembler.ToResourceFromEntity(checkout.Payment),
                InvoiceResourceFromEntityAssembler.ToResourceFromEntity(checkout.Invoice))));
    }
}
