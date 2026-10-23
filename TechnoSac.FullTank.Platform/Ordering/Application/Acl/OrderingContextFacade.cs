using TechnoSac.FullTank.Platform.Ordering.Application.QueryServices;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Ordering.Domain.Repositories;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Ordering.Application.Acl;

public class OrderingContextFacade(
    IOrderQueryService orderQueryService,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IOrderingContextFacade
{
    public async Task<bool> ExistsOrder(int orderId, CancellationToken cancellationToken)
    {
        return await Find(orderId, cancellationToken) is not null;
    }

    public async Task<int?> FetchOrderBuyerCompanyId(int orderId, CancellationToken cancellationToken)
    {
        return (await Find(orderId, cancellationToken))?.BuyerCompanyId;
    }

    public async Task<int?> FetchOrderProviderId(int orderId, CancellationToken cancellationToken)
    {
        return (await Find(orderId, cancellationToken))?.ProviderId;
    }

    public async Task<decimal?> FetchOrderTotalAmount(int orderId, CancellationToken cancellationToken)
    {
        return (await Find(orderId, cancellationToken))?.TotalAmount;
    }

    public async Task<string> FetchOrderStatus(int orderId, CancellationToken cancellationToken)
    {
        return (await Find(orderId, cancellationToken))?.Status ?? string.Empty;
    }

    public async Task<bool> MarkOrderPaid(int orderId, CancellationToken cancellationToken)
    {
        var order = await orderRepository.FindByIdAsync(orderId, cancellationToken);
        if (order is null) return false;

        order.MarkPaid();
        orderRepository.Update(order);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<(string FuelType, int Quantity, string Unit, decimal UnitPrice, decimal TotalAmount)?> FetchOrderLineInfo(int orderId, CancellationToken cancellationToken)
    {
        var order = await Find(orderId, cancellationToken);
        if (order is null) return null;
        return (order.FuelType, order.Quantity, order.Unit, order.UnitPrice, order.TotalAmount);
    }

    private async Task<Domain.Model.Aggregates.Order?> Find(int orderId, CancellationToken cancellationToken)
    {
        return await orderQueryService.Handle(new GetOrderByIdQuery(orderId), cancellationToken);
    }
}
