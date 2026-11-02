using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms a <see cref="ChangePasswordResource" /> into a <see cref="ChangePasswordCommand" />.</summary>
public static class ChangePasswordCommandFromResourceAssembler
{
    public static ChangePasswordCommand ToCommandFromResource(int userId, ChangePasswordResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new ChangePasswordCommand(userId, resource.CurrentPassword, resource.NewPassword);
    }
}
