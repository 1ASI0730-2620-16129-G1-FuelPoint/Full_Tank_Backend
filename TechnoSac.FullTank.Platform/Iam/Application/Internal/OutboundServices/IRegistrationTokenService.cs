namespace TechnoSac.FullTank.Platform.Iam.Application.Internal.OutboundServices;

/// <summary>Service for generating and verifying purpose-bound company registration tokens.</summary>
public interface IRegistrationTokenService
{
    string GenerateToken(int companyId, string role);
    bool ValidateToken(string? token, int companyId, string role);
}
