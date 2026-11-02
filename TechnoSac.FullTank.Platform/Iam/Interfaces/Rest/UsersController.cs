using System.Net.Mime;
using TechnoSac.FullTank.Platform.Iam.Application.CommandServices;
using TechnoSac.FullTank.Platform.Iam.Application.QueryServices;
using TechnoSac.FullTank.Platform.Iam.Domain.Model;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;
using TechnoSac.FullTank.Platform.Iam.Resources;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available User endpoints")]
public class UsersController(
    IUserQueryService userQueryService,
    IUserCommandService userCommandService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    IStringLocalizer<IamMessages> iamLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [Authorize("ADMIN")]
    [SwaggerOperation(Summary = "Get all users", Description = "Get all users", OperationId = "GetAllUsers")]
    [SwaggerResponse(StatusCodes.Status200OK, "The users were found", typeof(IEnumerable<UserResource>))]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await userQueryService.Handle(new GetAllUsersQuery(), cancellationToken);
        var resources = users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a user by id", Description = "Get a user by its id", OperationId = "GetUserById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was found", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The user was not found")]
    public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken)
    {
        if (this.CurrentUser().Id != id) return this.Forbidden();
        var user = await userQueryService.Handle(new GetUserByIdQuery(id), cancellationToken);

        return IamActionResultAssembler.ToActionResultFromEntity(
            this,
            user,
            IamError.UserNotFound,
            errorLocalizer,
            problemDetailsFactory,
            foundUser => Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(foundUser)));
    }

    [HttpPut("{id:int}/profile")]
    [SwaggerOperation(Summary = "Update user profile", Description = "Update the editable profile fields of a user",
        OperationId = "UpdateUserProfile")]
    [SwaggerResponse(StatusCodes.Status200OK, "The profile was updated", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The user was not found")]
    public async Task<IActionResult> UpdateUserProfile(int id, [FromBody] UpdateUserProfileResource resource,
        CancellationToken cancellationToken)
    {
        if (this.CurrentUser().Id != id) return this.Forbidden();
        var command = UpdateUserProfileCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await userCommandService.Handle(command, cancellationToken);

        return IamActionResultAssembler.ToActionResult(
            this,
            result,
            problemDetailsFactory,
            user => Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(user)));
    }

    [HttpPut("{id:int}/password")]
    [SwaggerOperation(Summary = "Change user password", Description = "Change a user's password after verifying the current one",
        OperationId = "ChangeUserPassword")]
    [SwaggerResponse(StatusCodes.Status200OK, "The password was changed")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The current password is invalid")]
    public async Task<IActionResult> ChangeUserPassword(int id, [FromBody] ChangePasswordResource resource,
        CancellationToken cancellationToken)
    {
        if (this.CurrentUser().Id != id) return this.Forbidden();
        var command = ChangePasswordCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await userCommandService.Handle(command, cancellationToken);

        return IamActionResultAssembler.ToActionResult(
            this,
            result,
            problemDetailsFactory,
            () => Ok(new { message = iamLocalizer["PasswordChangedSuccessfully"].Value }));
    }
}
