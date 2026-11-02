using TechnoSac.FullTank.Platform.Iam.Application.Internal.OutboundServices;
using BCryptNet = BCrypt.Net.BCrypt;

namespace TechnoSac.FullTank.Platform.Iam.Infrastructure.Hashing.BCrypt.Services;

/// <summary>BCrypt-based implementation of <see cref="IHashingService" />.</summary>
public class HashingService : IHashingService
{
    public string HashPassword(string password)
    {
        return BCryptNet.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCryptNet.Verify(password, passwordHash);
    }
}
