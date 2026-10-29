using TechnoSac.FullTank.Platform.Iam.Application.QueryServices;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Iam.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Iam.Application.Internal.QueryServices;

/// <summary>Handles user queries.</summary>
public class UserQueryService(IUserRepository userRepository) : IUserQueryService
{
    public async Task<User?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        return await userRepository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        return await userRepository.ListAsync(cancellationToken);
    }

    public async Task<User?> Handle(GetUserByUsernameQuery query, CancellationToken cancellationToken)
    {
        return await userRepository.FindByUsernameAsync(query.Username, cancellationToken);
    }
}
