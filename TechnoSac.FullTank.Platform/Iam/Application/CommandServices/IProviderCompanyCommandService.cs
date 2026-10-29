using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Iam.Application.CommandServices;

/// <summary>Command service contract for provider companies.</summary>
public interface IProviderCompanyCommandService
{
    Task<Result<ProviderCompany>> Handle(CreateProviderCompanyCommand command, CancellationToken cancellationToken);
    Task<Result<ProviderCompany>> Handle(UpdateProviderCompanyCommand command, CancellationToken cancellationToken);
}
