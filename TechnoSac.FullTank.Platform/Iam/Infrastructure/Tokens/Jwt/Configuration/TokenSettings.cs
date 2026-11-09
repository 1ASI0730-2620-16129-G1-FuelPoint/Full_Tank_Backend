namespace TechnoSac.FullTank.Platform.Iam.Infrastructure.Tokens.Jwt.Configuration;

/// <summary>
///     JWT token settings, bound from the <c>TokenSettings</c> section of appsettings.json.
/// </summary>
public class TokenSettings
{
    public required string Secret { get; set; }
}
