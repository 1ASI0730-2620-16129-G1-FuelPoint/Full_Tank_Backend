using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Catalog.Application.CommandServices;

public interface IProviderRatingCommandService
{
    Task<Result<ProviderRating>> Handle(CreateProviderRatingCommand command, CancellationToken cancellationToken);
    Task<Result<ProviderRating>> Handle(UpdateProviderRatingCommand command, CancellationToken cancellationToken);
}
