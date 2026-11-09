using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms a <see cref="SignInResource" /> into a <see cref="SignInCommand" />.</summary>
public static class SignInCommandFromResourceAssembler
{
    public static SignInCommand ToCommandFromResource(SignInResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new SignInCommand(resource.Email, resource.Password);
    }
}
