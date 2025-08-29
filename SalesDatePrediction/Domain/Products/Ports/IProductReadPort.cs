using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Domain.Products.Ports;

public interface IProductReadPort
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<PaginationResponse<Product>> GetPagedAsync(PaginationParams paginationParams, CancellationToken ct = default);
}
