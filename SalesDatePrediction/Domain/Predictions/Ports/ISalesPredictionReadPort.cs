using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Domain.Predictions.Ports;

public interface ISalesPredictionReadPort
{
    Task<IReadOnlyList<CustomerPrediction>> GetPredictionsAsync(CancellationToken ct = default);
    Task<PaginationResponse<CustomerPrediction>> GetPredictionsPagedAsync(PaginationParams paginationParams, CancellationToken ct = default);
}
