using MediatR;

namespace SalesDatePrediction.Application.Orders;

public sealed record OrderDetailReadDto(
    int ProductId, string ProductName, decimal UnitPrice, int Qty, decimal Discount);

public sealed record OrderReadDto(
    int OrderId, int? CustId, int EmpId, int ShipperId,
    DateTime OrderDate, DateTime RequiredDate, DateTime? ShippedDate,
    decimal Freight, string ShipName, string ShipAddress, string ShipCity, string ShipCountry,
    IReadOnlyList<OrderDetailReadDto> Details
);

public sealed record GetOrderByIdQuery(int OrderId) : IRequest<OrderReadDto?>;
