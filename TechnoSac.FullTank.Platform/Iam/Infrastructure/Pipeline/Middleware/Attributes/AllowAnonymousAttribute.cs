namespace TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;

/// <summary>
///     Decorates actions that do not require authorization, exempting them from the <c>[Authorize]</c> check.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AllowAnonymousAttribute : Attribute
{
}
