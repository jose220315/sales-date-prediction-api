namespace SalesDatePrediction.Domain.Orders.Ports;

public interface IOrderWritePort
{
    Task<int> AddAsync(CreateOrder order, CreateOrderDetail detail, CancellationToken ct = default);
}
