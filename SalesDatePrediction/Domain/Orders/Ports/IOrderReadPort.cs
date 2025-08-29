namespace SalesDatePrediction.Domain.Orders.Ports;

public interface IOrderReadPort
{
    Task<OrderRead?> GetByIdAsync(int orderId, CancellationToken ct = default);
}
