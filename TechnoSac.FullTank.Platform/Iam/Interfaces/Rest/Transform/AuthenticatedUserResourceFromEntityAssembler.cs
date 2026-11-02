using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms a <see cref="User" /> and a JWT token into an <see cref="AuthenticatedUserResource" />.</summary>
public static class AuthenticatedUserResourceFromEntityAssembler
{
    public static AuthenticatedUserResource ToResourceFromEntity(User user, string token)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        return new AuthenticatedUserResource(
            user.Id,
            user.Name,
            user.Email,
            user.Username,
            user.Role,
            user.CompanyId,
            token);
    }
}
