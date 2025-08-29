namespace SalesDatePrediction.Domain
{
    public sealed record CustomerPrediction(
        string CustomerName,
        DateTime LastOrderDate,
        DateTime NextPredictedOrder
);
}
