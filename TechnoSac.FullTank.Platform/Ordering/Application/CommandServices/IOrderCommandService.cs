using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Ordering.Application.CommandServices;

public interface IOrderCommandService
{
    Task<Result<Order>> Handle(CreateOrderCommand command, CancellationToken cancellationToken);
    Task<Result<Order>> Handle(UpdateOrderCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteOrderCommand command, CancellationToken cancellationToken);
    Task<Result<Order>> Handle(ApproveFuelRequestCommand command, CancellationToken cancellationToken);
    Task<Result<Order>> Handle(DispatchOrderCommand command, CancellationToken cancellationToken);
    Task<Result<Order>> Handle(ReceiveOrderCommand command, CancellationToken cancellationToken);
    Task<Result<Order>> Handle(CancelOrderCommand command, CancellationToken cancellationToken);
}
