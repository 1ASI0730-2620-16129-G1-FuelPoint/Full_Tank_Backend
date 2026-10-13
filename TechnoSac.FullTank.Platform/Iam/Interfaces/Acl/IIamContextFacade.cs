namespace TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;

/// <summary>
///     Anti-corruption facade exposing minimal, primitive-based IAM data to other bounded contexts.
///     Other contexts must use this facade instead of importing IAM aggregates.
/// </summary>
public interface IIamContextFacade
{
    /// <summary>Returns the id of the user with the given email, or 0 if none exists.</summary>
    Task<int> FetchUserIdByEmail(string email, CancellationToken cancellationToken);

    /// <summary>Returns whether a user exists with the given id.</summary>
    Task<bool> ExistsUser(int userId, CancellationToken cancellationToken);

    /// <summary>Returns the company id linked to the given user, or null.</summary>
    Task<int?> FetchCompanyIdByUserId(int userId, CancellationToken cancellationToken);

    /// <summary>Returns the role of the given user, or an empty string if none exists.</summary>
    Task<string> FetchUserRoleByUserId(int userId, CancellationToken cancellationToken);

    /// <summary>Returns whether a buyer company exists with the given id.</summary>
    Task<bool> ExistsBuyerCompany(int companyId, CancellationToken cancellationToken);

    /// <summary>Returns whether a provider company exists with the given id.</summary>
    Task<bool> ExistsProviderCompany(int providerId, CancellationToken cancellationToken);

    /// <summary>Returns the name and RUC of a buyer company, or nulls if not found.</summary>
    Task<(string? Name, string? Ruc)> FetchBuyerCompanyInfo(int companyId, CancellationToken cancellationToken);

    /// <summary>Returns the name and RUC of a provider company, or nulls if not found.</summary>
    Task<(string? Name, string? Ruc)> FetchProviderCompanyInfo(int providerId, CancellationToken cancellationToken);
}
