namespace SalesDatePrediction.Domain.Orders;

public sealed record CreateOrder(
    int? CustId,
    int EmpId,
    DateTime OrderDate,
    DateTime RequiredDate,
    DateTime? ShippedDate,
    int ShipperId,
    decimal Freight,
    string ShipName,
    string ShipAddress,
    string ShipCity,
    string ShipCountry
);

public sealed record CreateOrderDetail(
    int ProductId,
    decimal? UnitPrice,
    int Qty,
    decimal Discount
);
