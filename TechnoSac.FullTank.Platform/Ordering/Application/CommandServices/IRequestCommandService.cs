using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Ordering.Application.CommandServices;

public interface IRequestCommandService
{
    Task<Result<Request>> Handle(CreateRequestCommand command, CancellationToken cancellationToken);
    Task<Result<Request>> Handle(UpdateRequestCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteRequestCommand command, CancellationToken cancellationToken);
}
