namespace SalesDatePrediction.Domain.Orders.Ports;

public interface IOrderWritePort
{
    Task<int> AddAsync(CreateOrder order, IReadOnlyList<CreateOrderDetail> details, CancellationToken ct = default);
}
