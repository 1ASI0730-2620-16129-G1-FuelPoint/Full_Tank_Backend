using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Catalog.Application.CommandServices;

public interface IProviderProductCommandService
{
    Task<Result<ProviderProduct>> Handle(CreateProviderProductCommand command, CancellationToken cancellationToken);
    Task<Result<ProviderProduct>> Handle(UpdateProviderProductCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteProviderProductCommand command, CancellationToken cancellationToken);
}
