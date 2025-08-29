namespace SalesDatePrediction.Domain.Orders;

public sealed record OrderDetailRead(
    int ProductId,
    string ProductName,
    decimal UnitPrice,
    short Qty,
    decimal Discount
    );
public sealed record OrderRead(
    int OrderId,
    int? CustId,
    int EmpId,
    int ShipperId,
    DateTime OrderDate,
    DateTime RequiredDate,
    DateTime? ShippedDate,
    decimal Freight,
    string ShipName,
    string ShipAddress,
    string ShipCity,
    string ShipCountry,
    IReadOnlyList<OrderDetailRead> Details
);
