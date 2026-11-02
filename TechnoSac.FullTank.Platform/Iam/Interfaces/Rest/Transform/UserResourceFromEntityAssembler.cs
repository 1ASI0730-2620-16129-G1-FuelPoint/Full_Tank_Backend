using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms a <see cref="User" /> aggregate into a <see cref="UserResource" />.</summary>
public static class UserResourceFromEntityAssembler
{
    public static UserResource ToResourceFromEntity(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new UserResource(
            user.Id,
            user.Name,
            user.Email,
            user.Username,
            user.Role,
            user.CompanyId,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
