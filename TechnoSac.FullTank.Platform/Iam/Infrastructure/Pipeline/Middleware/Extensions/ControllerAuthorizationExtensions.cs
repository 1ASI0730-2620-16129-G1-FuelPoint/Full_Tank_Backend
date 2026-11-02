using Microsoft.AspNetCore.Mvc;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;

namespace TechnoSac.FullTank.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;

public static class ControllerAuthorizationExtensions
{
    public static User CurrentUser(this ControllerBase controller)
    {
        return (User)controller.HttpContext.Items["User"]!;
    }

    public static bool OwnsBuyerCompany(this ControllerBase controller, int? companyId)
    {
        var user = controller.CurrentUser();
        return user.Role == "BUYER" && companyId == user.CompanyId;
    }

    public static bool OwnsProviderCompany(this ControllerBase controller, int? providerId)
    {
        var user = controller.CurrentUser();
        return user.Role == "PROVIDER" && providerId == user.CompanyId;
    }

    public static IActionResult Forbidden(this ControllerBase controller)
    {
        return new StatusCodeResult(StatusCodes.Status403Forbidden);
    }
}
