using MediatR;

namespace SalesDatePrediction.Application.Orders;

public sealed record CreateOrderDetailDto(int ProductId, decimal? UnitPrice, int Qty, decimal Discount);

public sealed record CreateOrderCommand(
    int? CustId,
    int EmpId,
    int ShipperId,
    string ShipName,
    string ShipAddress,
    string ShipCity,
    string ShipCountry,
    DateTime? OrderDate,
    DateTime? RequiredDate,
    DateTime? ShippedDate,
    decimal Freight,
    CreateOrderDetailDto Detail
) : IRequest<int>;
