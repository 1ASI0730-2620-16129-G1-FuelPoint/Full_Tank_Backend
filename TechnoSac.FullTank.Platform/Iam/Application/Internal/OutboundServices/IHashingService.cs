namespace TechnoSac.FullTank.Platform.Iam.Application.Internal.OutboundServices;

/// <summary>Outbound port for password hashing and verification.</summary>
public interface IHashingService
{
    /// <summary>Hashes a plain-text password.</summary>
    string HashPassword(string password);

    /// <summary>Verifies a plain-text password against a stored hash.</summary>
    bool VerifyPassword(string password, string passwordHash);
}
