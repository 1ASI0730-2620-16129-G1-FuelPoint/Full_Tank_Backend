using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Iam.Application.CommandServices;

/// <summary>Command service contract for user authentication and profile operations.</summary>
public interface IUserCommandService
{
    /// <summary>Handles a sign-in command, returning the authenticated user and a JWT token.</summary>
    Task<Result<(User user, string token)>> Handle(SignInCommand command, CancellationToken cancellationToken);

    /// <summary>Handles a sign-up command, returning the created user.</summary>
    Task<Result<User>> Handle(SignUpCommand command, CancellationToken cancellationToken);

    /// <summary>Handles a profile update command, returning the updated user.</summary>
    Task<Result<User>> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken);

    /// <summary>Handles a change-password command.</summary>
    Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken);
}
