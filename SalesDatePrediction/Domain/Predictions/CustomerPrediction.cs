namespace SalesDatePrediction.Domain.Predictions
{
    public sealed record CustomerPrediction(
        string CustomerName,
        DateTime LastOrderDate,
        DateTime NextPredictedOrder
);
}
