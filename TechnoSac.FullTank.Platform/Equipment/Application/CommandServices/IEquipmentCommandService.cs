using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;

namespace TechnoSac.FullTank.Platform.Equipment.Application.CommandServices;

public interface IEquipmentCommandService
{
    Task<Result<EquipmentAggregate>> Handle(CreateEquipmentCommand command, CancellationToken cancellationToken);
    Task<Result<EquipmentAggregate>> Handle(UpdateEquipmentCommand command, CancellationToken cancellationToken);
}
