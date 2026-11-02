using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Iam.Application.QueryServices;

/// <summary>Query service contract for users.</summary>
public interface IUserQueryService
{
    Task<User?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken);
    Task<User?> Handle(GetUserByUsernameQuery query, CancellationToken cancellationToken);
}
