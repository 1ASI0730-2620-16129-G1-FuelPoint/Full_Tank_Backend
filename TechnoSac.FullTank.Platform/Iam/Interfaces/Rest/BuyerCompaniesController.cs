using System.Net.Mime;
using TechnoSac.FullTank.Platform.Iam.Application.CommandServices;
using TechnoSac.FullTank.Platform.Iam.Application.Internal.OutboundServices;
using TechnoSac.FullTank.Platform.Iam.Application.QueryServices;
using TechnoSac.FullTank.Platform.Iam.Domain.Model;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest;

[ApiController]
[Authorize("BUYER", "PROVIDER")]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Buyer Company endpoints")]
public class BuyerCompaniesController(
    IBuyerCompanyCommandService buyerCompanyCommandService,
    IBuyerCompanyQueryService buyerCompanyQueryService,
    IRegistrationTokenService registrationTokenService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get all buyer companies", OperationId = "GetAllBuyerCompanies")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<BuyerCompanyResource>))]
    public async Task<IActionResult> GetAllBuyerCompanies(CancellationToken cancellationToken)
    {
        var companies = await buyerCompanyQueryService.Handle(new GetAllBuyerCompaniesQuery(), cancellationToken);
        var resources = companies.Select(company => BuyerCompanyResourceFromEntityAssembler.ToResourceFromEntity(company));
        return Ok(resources);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a buyer company by id", OperationId = "GetBuyerCompanyById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The buyer company was found", typeof(BuyerCompanyResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The buyer company was not found")]
    public async Task<IActionResult> GetBuyerCompanyById(int id, CancellationToken cancellationToken)
    {
        var company = await buyerCompanyQueryService.Handle(new GetBuyerCompanyByIdQuery(id), cancellationToken);

        return IamActionResultAssembler.ToActionResultFromEntity(
            this,
            company,
            IamError.BuyerCompanyNotFound,
            errorLocalizer,
            problemDetailsFactory,
            found => Ok(BuyerCompanyResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Create a buyer company", OperationId = "CreateBuyerCompany")]
    [SwaggerResponse(StatusCodes.Status201Created, "The buyer company was created", typeof(BuyerCompanyResource))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "The RUC is already registered")]
    public async Task<IActionResult> CreateBuyerCompany([FromBody] CreateBuyerCompanyResource resource,
        CancellationToken cancellationToken)
    {
        var command = CreateBuyerCompanyCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await buyerCompanyCommandService.Handle(command, cancellationToken);

        return IamActionResultAssembler.ToActionResult(
            this,
            result,
            problemDetailsFactory,
            company =>
            {
                var token = registrationTokenService.GenerateToken(company.Id, "BUYER");
                return CreatedAtAction(nameof(GetBuyerCompanyById), new { id = company.Id },
                    BuyerCompanyResourceFromEntityAssembler.ToResourceFromEntity(company, token));
            });
    }

    [HttpPut("{id:int}")]
    [Authorize("BUYER")]
    [SwaggerOperation(Summary = "Update a buyer company", OperationId = "UpdateBuyerCompany")]
    [SwaggerResponse(StatusCodes.Status200OK, "The buyer company was updated", typeof(BuyerCompanyResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The buyer company was not found")]
    public async Task<IActionResult> UpdateBuyerCompany(int id, [FromBody] UpdateBuyerCompanyResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsBuyerCompany(id)) return this.Forbidden();
        var command = UpdateBuyerCompanyCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await buyerCompanyCommandService.Handle(command, cancellationToken);

        return IamActionResultAssembler.ToActionResult(
            this,
            result,
            problemDetailsFactory,
            company => Ok(BuyerCompanyResourceFromEntityAssembler.ToResourceFromEntity(company)));
    }
}
