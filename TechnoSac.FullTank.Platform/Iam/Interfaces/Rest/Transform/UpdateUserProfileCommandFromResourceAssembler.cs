using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms an <see cref="UpdateUserProfileResource" /> into an <see cref="UpdateUserProfileCommand" />.</summary>
public static class UpdateUserProfileCommandFromResourceAssembler
{
    public static UpdateUserProfileCommand ToCommandFromResource(int userId, UpdateUserProfileResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateUserProfileCommand(userId, resource.Name, resource.Email);
    }
}
