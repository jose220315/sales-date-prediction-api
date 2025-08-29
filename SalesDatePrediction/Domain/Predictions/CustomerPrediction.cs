namespace SalesDatePrediction.Domain.Predictions
{
    public sealed record CustomerPrediction(
        int CustomerId,
        string CustomerName,
        DateTime LastOrderDate,
        DateTime NextPredictedOrder
);
}
