namespace SalesDatePrediction.Domain.Products.Ports;

public interface IProductReadPort
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
}
