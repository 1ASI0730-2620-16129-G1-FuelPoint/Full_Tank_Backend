using TechnoSac.FullTank.Platform.Catalog.Application.CommandServices;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Catalog.Application.Internal.CommandServices;

public class ProviderProductCommandService(
    IProviderProductRepository repository,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IProviderProductCommandService
{
    public async Task<Result<ProviderProduct>> Handle(CreateProviderProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = new ProviderProduct(command);
        try
        {
            await repository.AddAsync(product, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<ProviderProduct>.Success(product);
        }
        catch (DbUpdateException)
        {
            return Result<ProviderProduct>.Failure(CatalogError.DatabaseError,
                localizer[nameof(CatalogError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<ProviderProduct>.Failure(CatalogError.InternalServerError,
                localizer[nameof(CatalogError.InternalServerError)]);
        }
    }

    public async Task<Result<ProviderProduct>> Handle(UpdateProviderProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (product is null)
            return Result<ProviderProduct>.Failure(CatalogError.NotFound, localizer[nameof(CatalogError.NotFound)]);

        try
        {
            product.Update(command);
            repository.Update(product);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<ProviderProduct>.Success(product);
        }
        catch (Exception)
        {
            return Result<ProviderProduct>.Failure(CatalogError.InternalServerError,
                localizer[nameof(CatalogError.InternalServerError)]);
        }
    }

    public async Task<Result> Handle(DeleteProviderProductCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (product is null)
            return Result.Failure(CatalogError.NotFound, localizer[nameof(CatalogError.NotFound)]);

        try
        {
            repository.Remove(product);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(CatalogError.InternalServerError, localizer[nameof(CatalogError.InternalServerError)]);
        }
    }
}
