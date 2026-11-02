using TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Components;

namespace TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;

/// <summary>Registers <see cref="RequestAuthorizationMiddleware" /> in the ASP.NET Core pipeline.</summary>
public static class RequestAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestAuthorizationMiddleware>();
    }
}
