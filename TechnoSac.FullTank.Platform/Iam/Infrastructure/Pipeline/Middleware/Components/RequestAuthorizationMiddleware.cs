using TechnoSac.FullTank.Platform.Iam.Application.Internal.OutboundServices;
using TechnoSac.FullTank.Platform.Iam.Application.QueryServices;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Components;

/// <summary>
///     Best-effort token reader. If a valid JWT is present, it resolves the user and stores it in
///     <c>HttpContext.Items["User"]</c>. It never blocks the request — access is enforced by
///     <c>AuthorizeAttribute</c>, which returns 401 when no user was resolved.
/// </summary>
public class RequestAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IUserQueryService userQueryService,
        ITokenService tokenService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var userId = await tokenService.ValidateToken(token);
            if (userId != null)
            {
                var user = await userQueryService.Handle(new GetUserByIdQuery(userId.Value), context.RequestAborted);
                if (user != null)
                    context.Items["User"] = user;
            }
        }

        await next(context);
    }
}
