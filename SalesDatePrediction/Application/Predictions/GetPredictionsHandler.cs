using MediatR;
using SalesDatePrediction.Domain.Predictions.Ports;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Application.Predictions;

public sealed class GetPredictionsHandler(ISalesPredictionReadPort readPort)
        : IRequestHandler<GetPredictionsQuery, PaginationResponse<CustomerPredictionDto>>
{
    public async Task<PaginationResponse<CustomerPredictionDto>> Handle(
        GetPredictionsQuery request, CancellationToken ct)
    {
        if (request.PaginationParams != null)
        {
            var pagedResult = await readPort.GetPredictionsPagedAsync(request.PaginationParams, ct);
            return new PaginationResponse<CustomerPredictionDto>
            {
                Data = pagedResult.Data.Select(x => new CustomerPredictionDto(x.CustomerId, x.CustomerName, x.LastOrderDate, x.NextPredictedOrder)).ToList(),
                TotalPages = pagedResult.TotalPages,
                TotalRows = pagedResult.TotalRows
            };
        }
        else
        {
            var allPredictions = await readPort.GetPredictionsAsync(ct);
            return new PaginationResponse<CustomerPredictionDto>
            {
                Data = allPredictions.Select(x => new CustomerPredictionDto(x.CustomerId, x.CustomerName, x.LastOrderDate, x.NextPredictedOrder)).ToList(),
                TotalPages = 1,
                TotalRows = allPredictions.Count
            };
        }
    }
}
