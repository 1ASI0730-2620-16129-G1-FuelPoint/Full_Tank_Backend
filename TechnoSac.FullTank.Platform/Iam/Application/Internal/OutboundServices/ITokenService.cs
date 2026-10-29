using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;

namespace TechnoSac.FullTank.Platform.Iam.Application.Internal.OutboundServices;

/// <summary>Outbound port for JWT generation and validation.</summary>
public interface ITokenService
{
    /// <summary>Generates a JWT for the given user (claims: id, email, username, role, companyId).</summary>
    string GenerateToken(User user);

    /// <summary>Validates a JWT and returns the user id if valid, otherwise null.</summary>
    Task<int?> ValidateToken(string token);
}
