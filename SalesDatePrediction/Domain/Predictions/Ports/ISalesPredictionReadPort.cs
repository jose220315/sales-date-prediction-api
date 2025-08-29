namespace SalesDatePrediction.Domain.Predictions.Ports;

public interface ISalesPredictionReadPort
{
    Task<IReadOnlyList<CustomerPrediction>> GetPredictionsAsync(CancellationToken ct = default);
}
