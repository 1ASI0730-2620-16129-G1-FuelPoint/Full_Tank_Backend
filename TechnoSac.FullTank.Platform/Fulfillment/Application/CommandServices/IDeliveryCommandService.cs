using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.CommandServices;

public interface IDeliveryCommandService
{
    Task<Result<Delivery>> Handle(CreateDeliveryCommand command, CancellationToken cancellationToken);
    Task<Result<Delivery>> Handle(UpdateDeliveryCommand command, CancellationToken cancellationToken);
    Task<Result<Delivery>> Handle(CompleteDeliveryCommand command, CancellationToken cancellationToken);
}
