using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Iam.Domain.Repositories;

/// <summary>Repository contract for the <see cref="User" /> aggregate.</summary>
public interface IUserRepository : IBaseRepository<User>
{
    /// <summary>Finds a user by its email (primary sign-in identifier).</summary>
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Checks whether a user already exists with the given email.</summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Finds a user by its username.</summary>
    Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken);

    /// <summary>Checks whether a user already exists with the given username.</summary>
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken);

    /// <summary>Checks whether a user already exists associated with the specified company and role.</summary>
    Task<bool> ExistsByCompanyIdAndRoleAsync(int companyId, string role, CancellationToken cancellationToken);
}
