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
[SwaggerTag("Available Provider Company endpoints")]
public class ProviderCompaniesController(
    IProviderCompanyCommandService providerCompanyCommandService,
    IProviderCompanyQueryService providerCompanyQueryService,
    IRegistrationTokenService registrationTokenService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get all provider companies", OperationId = "GetAllProviderCompanies")]
    [SwaggerResponse(StatusCodes.Status200OK, "The provider companies were found",
        typeof(IEnumerable<ProviderCompanyResource>))]
    public async Task<IActionResult> GetAllProviderCompanies(CancellationToken cancellationToken)
    {
        var companies = await providerCompanyQueryService.Handle(new GetAllProviderCompaniesQuery(), cancellationToken);
        var resources = companies.Select(company => ProviderCompanyResourceFromEntityAssembler.ToResourceFromEntity(company));
        return Ok(resources);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a provider company by id", OperationId = "GetProviderCompanyById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The provider company was found", typeof(ProviderCompanyResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The provider company was not found")]
    public async Task<IActionResult> GetProviderCompanyById(int id, CancellationToken cancellationToken)
    {
        var company = await providerCompanyQueryService.Handle(new GetProviderCompanyByIdQuery(id), cancellationToken);

        return IamActionResultAssembler.ToActionResultFromEntity(
            this,
            company,
            IamError.ProviderCompanyNotFound,
            errorLocalizer,
            problemDetailsFactory,
            found => Ok(ProviderCompanyResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Create a provider company", OperationId = "CreateProviderCompany")]
    [SwaggerResponse(StatusCodes.Status201Created, "The provider company was created", typeof(ProviderCompanyResource))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "The RUC is already registered")]
    public async Task<IActionResult> CreateProviderCompany([FromBody] CreateProviderCompanyResource resource,
        CancellationToken cancellationToken)
    {
        var command = CreateProviderCompanyCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await providerCompanyCommandService.Handle(command, cancellationToken);

        return IamActionResultAssembler.ToActionResult(
            this,
            result,
            problemDetailsFactory,
            company =>
            {
                var token = registrationTokenService.GenerateToken(company.Id, "PROVIDER");
                return CreatedAtAction(nameof(GetProviderCompanyById), new { id = company.Id },
                    ProviderCompanyResourceFromEntityAssembler.ToResourceFromEntity(company, token));
            });
    }

    [HttpPut("{id:int}")]
    [Authorize("PROVIDER")]
    [SwaggerOperation(Summary = "Update a provider company", OperationId = "UpdateProviderCompany")]
    [SwaggerResponse(StatusCodes.Status200OK, "The provider company was updated", typeof(ProviderCompanyResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The provider company was not found")]
    public async Task<IActionResult> UpdateProviderCompany(int id, [FromBody] UpdateProviderCompanyResource resource,
        CancellationToken cancellationToken)
    {
        if (!this.OwnsProviderCompany(id)) return this.Forbidden();
        var command = UpdateProviderCompanyCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await providerCompanyCommandService.Handle(command, cancellationToken);

        return IamActionResultAssembler.ToActionResult(
            this,
            result,
            problemDetailsFactory,
            company => Ok(ProviderCompanyResourceFromEntityAssembler.ToResourceFromEntity(company)));
    }
}
