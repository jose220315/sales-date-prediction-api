namespace SalesDatePrediction.Domain.Orders;

public sealed record ClientOrderSummary(
    int OrderId,
    DateTime RequiredDate,
    DateTime? ShippedDate,
    string ShipName,
    string ShipAddress,
    string ShipCity
);
