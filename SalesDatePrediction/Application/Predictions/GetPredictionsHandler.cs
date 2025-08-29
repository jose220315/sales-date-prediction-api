using MediatR;
using SalesDatePrediction.Domain.Predictions.Ports;

namespace SalesDatePrediction.Application.Predictions;

public sealed class GetPredictionsHandler
    : IRequestHandler<GetPredictionsQuery, IReadOnlyList<CustomerPredictionDto>>
{
    private readonly ISalesPredictionReadPort _readPort;

    public GetPredictionsHandler(ISalesPredictionReadPort readPort) => _readPort = readPort;

    public async Task<IReadOnlyList<CustomerPredictionDto>> Handle(
        GetPredictionsQuery request, CancellationToken ct)
    {
        var items = await _readPort.GetPredictionsAsync(ct);
        return items
            .Select(x => new CustomerPredictionDto(x.CustomerName, x.LastOrderDate, x.NextPredictedOrder))
            .ToList();
    }
}
