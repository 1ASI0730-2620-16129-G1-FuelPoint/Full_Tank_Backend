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

public class FavoriteProviderCommandService(
    IFavoriteProviderRepository repository,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IFavoriteProviderCommandService
{
    public async Task<Result<FavoriteProvider>> Handle(CreateFavoriteProviderCommand command,
        CancellationToken cancellationToken)
    {
        if (command.CompanyId <= 0 || command.ProviderId <= 0)
            return ValidationFailure();
        if (!await iamContextFacade.ExistsBuyerCompany(command.CompanyId, cancellationToken)
            || !await iamContextFacade.ExistsProviderCompany(command.ProviderId, cancellationToken))
            return ValidationFailure();
        if (await repository.FindByCompanyAndProviderAsync(command.CompanyId, command.ProviderId, cancellationToken)
            is not null)
            return ValidationFailure();

        var favorite = new FavoriteProvider(command);
        try
        {
            await repository.AddAsync(favorite, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<FavoriteProvider>.Success(favorite);
        }
        catch (DbUpdateException)
        {
            return ValidationFailure();
        }
        catch (Exception)
        {
            return Result<FavoriteProvider>.Failure(CatalogError.InternalServerError,
                localizer[nameof(CatalogError.InternalServerError)]);
        }
    }

    private Result<FavoriteProvider> ValidationFailure()
    {
        return Result<FavoriteProvider>.Failure(CatalogError.ValidationError,
            localizer[nameof(CatalogError.ValidationError)]);
    }

    public async Task<Result> Handle(DeleteFavoriteProviderCommand command, CancellationToken cancellationToken)
    {
        var favorite = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (favorite is null)
            return Result.Failure(CatalogError.NotFound, localizer[nameof(CatalogError.NotFound)]);

        try
        {
            repository.Remove(favorite);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(CatalogError.InternalServerError, localizer[nameof(CatalogError.InternalServerError)]);
        }
    }
}
