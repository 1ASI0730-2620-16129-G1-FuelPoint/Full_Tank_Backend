using TechnoSac.FullTank.Platform.Iam.Application.QueryServices;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;

namespace TechnoSac.FullTank.Platform.Iam.Application.Acl;

/// <summary>Implementation of <see cref="IIamContextFacade" /> backed by the IAM query services.</summary>
public class IamContextFacade(
    IUserQueryService userQueryService,
    IBuyerCompanyQueryService buyerCompanyQueryService,
    IProviderCompanyQueryService providerCompanyQueryService) : IIamContextFacade
{
    public async Task<int> FetchUserIdByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await userQueryService.Handle(new GetUserByUsernameQuery(email), cancellationToken);
        return user?.Id ?? 0;
    }

    public async Task<bool> ExistsUser(int userId, CancellationToken cancellationToken)
    {
        var user = await userQueryService.Handle(new GetUserByIdQuery(userId), cancellationToken);
        return user is not null;
    }

    public async Task<int?> FetchCompanyIdByUserId(int userId, CancellationToken cancellationToken)
    {
        var user = await userQueryService.Handle(new GetUserByIdQuery(userId), cancellationToken);
        return user?.CompanyId;
    }

    public async Task<string> FetchUserRoleByUserId(int userId, CancellationToken cancellationToken)
    {
        var user = await userQueryService.Handle(new GetUserByIdQuery(userId), cancellationToken);
        return user?.Role ?? string.Empty;
    }

    public async Task<bool> ExistsBuyerCompany(int companyId, CancellationToken cancellationToken)
    {
        var company = await buyerCompanyQueryService.Handle(new GetBuyerCompanyByIdQuery(companyId),
            cancellationToken);
        return company is not null;
    }

    public async Task<bool> ExistsProviderCompany(int providerId, CancellationToken cancellationToken)
    {
        var company = await providerCompanyQueryService.Handle(new GetProviderCompanyByIdQuery(providerId),
            cancellationToken);
        return company is not null;
    }

    public async Task<(string? Name, string? Ruc)> FetchBuyerCompanyInfo(int companyId, CancellationToken cancellationToken)
    {
        var company = await buyerCompanyQueryService.Handle(new GetBuyerCompanyByIdQuery(companyId), cancellationToken);
        return (company?.Name, company?.Ruc);
    }

    public async Task<(string? Name, string? Ruc)> FetchProviderCompanyInfo(int providerId, CancellationToken cancellationToken)
    {
        var company = await providerCompanyQueryService.Handle(new GetProviderCompanyByIdQuery(providerId), cancellationToken);
        return (company?.Name, company?.Ruc);
    }
}
