using TechnoSac.FullTank.Platform.Iam.Application.CommandServices;
using TechnoSac.FullTank.Platform.Iam.Domain.Model;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Domain.Repositories;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Iam.Application.Internal.CommandServices;

/// <summary>Handles provider company commands.</summary>
public class ProviderCompanyCommandService(
    IProviderCompanyRepository providerCompanyRepository,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IProviderCompanyCommandService
{
    public async Task<Result<ProviderCompany>> Handle(CreateProviderCompanyCommand command,
        CancellationToken cancellationToken)
    {
        if (await providerCompanyRepository.ExistsByRucAsync(command.Ruc, cancellationToken))
            return Result<ProviderCompany>.Failure(IamError.RucAlreadyTaken,
                localizer[nameof(IamError.RucAlreadyTaken), command.Ruc]);

        var company = new ProviderCompany(command);
        try
        {
            await providerCompanyRepository.AddAsync(company, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<ProviderCompany>.Success(company);
        }
        catch (OperationCanceledException)
        {
            return Result<ProviderCompany>.Failure(IamError.OperationCancelled,
                localizer[nameof(IamError.OperationCancelled)]);
        }
        catch (DbUpdateException)
        {
            return Result<ProviderCompany>.Failure(IamError.DatabaseError, localizer[nameof(IamError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<ProviderCompany>.Failure(IamError.InternalServerError,
                localizer[nameof(IamError.InternalServerError)]);
        }
    }

    public async Task<Result<ProviderCompany>> Handle(UpdateProviderCompanyCommand command,
        CancellationToken cancellationToken)
    {
        var company = await providerCompanyRepository.FindByIdAsync(command.Id, cancellationToken);
        if (company is null)
            return Result<ProviderCompany>.Failure(IamError.ProviderCompanyNotFound,
                localizer[nameof(IamError.ProviderCompanyNotFound)]);

        try
        {
            company.Update(command);
            providerCompanyRepository.Update(company);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<ProviderCompany>.Success(company);
        }
        catch (DbUpdateException)
        {
            return Result<ProviderCompany>.Failure(IamError.DatabaseError, localizer[nameof(IamError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<ProviderCompany>.Failure(IamError.InternalServerError,
                localizer[nameof(IamError.InternalServerError)]);
        }
    }
}
