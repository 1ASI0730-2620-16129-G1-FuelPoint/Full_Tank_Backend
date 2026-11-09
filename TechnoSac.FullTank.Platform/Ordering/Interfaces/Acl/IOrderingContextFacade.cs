namespace TechnoSac.FullTank.Platform.Ordering.Interfaces.Acl;

public interface IOrderingContextFacade
{
    Task<bool> ExistsOrder(int orderId, CancellationToken cancellationToken);
    Task<int?> FetchOrderBuyerCompanyId(int orderId, CancellationToken cancellationToken);
    Task<int?> FetchOrderProviderId(int orderId, CancellationToken cancellationToken);
    Task<decimal?> FetchOrderTotalAmount(int orderId, CancellationToken cancellationToken);
    Task<string> FetchOrderStatus(int orderId, CancellationToken cancellationToken);
    Task<bool> MarkOrderPaid(int orderId, CancellationToken cancellationToken);
    Task<(string FuelType, int Quantity, string Unit, decimal UnitPrice, decimal TotalAmount)?> FetchOrderLineInfo(int orderId, CancellationToken cancellationToken);
}
