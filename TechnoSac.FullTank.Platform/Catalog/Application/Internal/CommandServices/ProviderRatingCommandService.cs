using TechnoSac.FullTank.Platform.Catalog.Application.CommandServices;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Catalog.Application.Internal.CommandServices;

public class ProviderRatingCommandService(
    IProviderRatingRepository repository,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IProviderRatingCommandService
{
    public async Task<Result<ProviderRating>> Handle(CreateProviderRatingCommand command,
        CancellationToken cancellationToken)
    {
        if (!IsValid(command.CompanyId, command.ProviderId, command.Rating))
            return ValidationFailure();
        if (!await ReferencesExist(command.CompanyId, command.ProviderId, cancellationToken))
            return ValidationFailure();

        var existing = await repository.FindByCompanyAndProviderAsync(command.CompanyId, command.ProviderId,
            cancellationToken);
        if (existing is not null)
            return ValidationFailure();

        try
        {
            var rating = new ProviderRating(command);
            await repository.AddAsync(rating, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<ProviderRating>.Success(rating);
        }
        catch (DbUpdateException)
        {
            return Result<ProviderRating>.Failure(CatalogError.ValidationError,
                localizer[nameof(CatalogError.ValidationError)]);
        }
        catch (Exception)
        {
            return Result<ProviderRating>.Failure(CatalogError.InternalServerError,
                localizer[nameof(CatalogError.InternalServerError)]);
        }
    }

    public async Task<Result<ProviderRating>> Handle(UpdateProviderRatingCommand command,
        CancellationToken cancellationToken)
    {
        if (!IsValid(command.CompanyId, command.ProviderId, command.Rating))
            return ValidationFailure();
        if (!await ReferencesExist(command.CompanyId, command.ProviderId, cancellationToken))
            return ValidationFailure();

        var rating = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (rating is null)
            return Result<ProviderRating>.Failure(CatalogError.NotFound, localizer[nameof(CatalogError.NotFound)]);

        var duplicate = await repository.FindByCompanyAndProviderAsync(command.CompanyId, command.ProviderId,
            cancellationToken);
        if (duplicate is not null && duplicate.Id != command.Id)
            return ValidationFailure();

        try
        {
            rating.Update(command);
            repository.Update(rating);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<ProviderRating>.Success(rating);
        }
        catch (DbUpdateException)
        {
            return Result<ProviderRating>.Failure(CatalogError.ValidationError,
                localizer[nameof(CatalogError.ValidationError)]);
        }
        catch (Exception)
        {
            return Result<ProviderRating>.Failure(CatalogError.InternalServerError,
                localizer[nameof(CatalogError.InternalServerError)]);
        }
    }

    private static bool IsValid(int companyId, int providerId, int rating)
    {
        return companyId > 0 && providerId > 0 && rating is >= 1 and <= 5;
    }

    private async Task<bool> ReferencesExist(int companyId, int providerId, CancellationToken cancellationToken)
    {
        return await iamContextFacade.ExistsBuyerCompany(companyId, cancellationToken)
               && await iamContextFacade.ExistsProviderCompany(providerId, cancellationToken);
    }

    private Result<ProviderRating> ValidationFailure()
    {
        return Result<ProviderRating>.Failure(CatalogError.ValidationError,
            localizer[nameof(CatalogError.ValidationError)]);
    }
}
