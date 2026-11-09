using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;

/// <summary>
///     Authorization filter that requires an authenticated user. It reads the user resolved by
///     <c>RequestAuthorizationMiddleware</c> from <c>HttpContext.Items["User"]</c> and returns 401 when absent,
///     unless the action is decorated with <see cref="AllowAnonymousAttribute" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly HashSet<string> roles;

    public AuthorizeAttribute(params string[] roles)
    {
        this.roles = roles.Select(role => role.ToUpperInvariant()).ToHashSet();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous)
            return;

        var user = (User?)context.HttpContext.Items["User"];
        if (user == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (roles.Count > 0 && !roles.Contains(user.Role.ToUpperInvariant()))
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
    }
}
