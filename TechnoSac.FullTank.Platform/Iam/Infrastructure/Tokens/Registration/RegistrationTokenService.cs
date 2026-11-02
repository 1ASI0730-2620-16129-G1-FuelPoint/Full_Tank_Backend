using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using TechnoSac.FullTank.Platform.Iam.Application.Internal.OutboundServices;

namespace TechnoSac.FullTank.Platform.Iam.Infrastructure.Tokens.Registration;

public class RegistrationTokenService : IRegistrationTokenService
{
    private readonly IDataProtector _protector;

    public RegistrationTokenService(IDataProtectionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _protector = provider.CreateProtector("FullTank.CompanyRegistrationToken.v1");
    }

    private sealed record TokenPayload(int CompanyId, string Role, long ExpiresAtUnix);

    public string GenerateToken(int companyId, string role)
    {
        var payload = new TokenPayload(
            companyId,
            role.ToUpperInvariant(),
            DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds());

        var json = JsonSerializer.Serialize(payload);
        return _protector.Protect(json);
    }

    public bool ValidateToken(string? token, int companyId, string role)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        try
        {
            var json = _protector.Unprotect(token);
            var payload = JsonSerializer.Deserialize<TokenPayload>(json);
            if (payload is null) return false;

            if (payload.ExpiresAtUnix < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                return false;

            return payload.CompanyId == companyId &&
                   string.Equals(payload.Role, role, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
