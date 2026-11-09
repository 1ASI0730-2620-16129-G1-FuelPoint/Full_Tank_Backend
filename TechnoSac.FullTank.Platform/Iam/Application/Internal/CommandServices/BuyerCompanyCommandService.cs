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

/// <summary>Handles buyer company commands.</summary>
public class BuyerCompanyCommandService(
    IBuyerCompanyRepository buyerCompanyRepository,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IBuyerCompanyCommandService
{
    public async Task<Result<BuyerCompany>> Handle(CreateBuyerCompanyCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await buyerCompanyRepository.ExistsByRucAsync(command.Ruc, cancellationToken))
                return Result<BuyerCompany>.Failure(IamError.RucAlreadyTaken,
                    localizer[nameof(IamError.RucAlreadyTaken), command.Ruc]);

            var company = new BuyerCompany(command);
            await buyerCompanyRepository.AddAsync(company, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<BuyerCompany>.Success(company);
        }
        catch (OperationCanceledException)
        {
            return Result<BuyerCompany>.Failure(IamError.OperationCancelled,
                localizer[nameof(IamError.OperationCancelled)]);
        }
        catch (DbUpdateException)
        {
            return Result<BuyerCompany>.Failure(IamError.DatabaseError, localizer[nameof(IamError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<BuyerCompany>.Failure(IamError.InternalServerError,
                localizer[nameof(IamError.InternalServerError)]);
        }
    }

    public async Task<Result<BuyerCompany>> Handle(UpdateBuyerCompanyCommand command,
        CancellationToken cancellationToken)
    {
        var company = await buyerCompanyRepository.FindByIdAsync(command.Id, cancellationToken);
        if (company is null)
            return Result<BuyerCompany>.Failure(IamError.BuyerCompanyNotFound,
                localizer[nameof(IamError.BuyerCompanyNotFound)]);

        try
        {
            company.Update(command);
            buyerCompanyRepository.Update(company);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<BuyerCompany>.Success(company);
        }
        catch (DbUpdateException)
        {
            return Result<BuyerCompany>.Failure(IamError.DatabaseError, localizer[nameof(IamError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<BuyerCompany>.Failure(IamError.InternalServerError,
                localizer[nameof(IamError.InternalServerError)]);
        }
    }
}
