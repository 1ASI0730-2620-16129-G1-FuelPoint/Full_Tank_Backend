using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Equipment.Application.CommandServices;

public interface IRefillHistoryCommandService
{
    Task<Result<RefillHistory>> Handle(CreateRefillHistoryCommand command, CancellationToken cancellationToken);
}
