using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Catalog.Application.CommandServices;

public interface IFavoriteProviderCommandService
{
    Task<Result<FavoriteProvider>> Handle(CreateFavoriteProviderCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteFavoriteProviderCommand command, CancellationToken cancellationToken);
}
