using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms a <see cref="SignUpResource" /> into a <see cref="SignUpCommand" />.</summary>
public static class SignUpCommandFromResourceAssembler
{
    public static SignUpCommand ToCommandFromResource(SignUpResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new SignUpCommand(
            resource.Name,
            resource.Email,
            resource.Username,
            resource.Password,
            resource.Role,
            resource.CompanyId,
            resource.RegistrationToken);
    }
}
