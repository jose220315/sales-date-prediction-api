using MediatR;
using SalesDatePrediction.Domain.Orders.Ports;

namespace SalesDatePrediction.Application.Orders;

public sealed class GetOrderByIdHandler(IOrderReadPort port) : IRequestHandler<GetOrderByIdQuery, OrderReadDto?>
{
    public async Task<OrderReadDto?> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var o = await port.GetByIdAsync(request.OrderId, ct);
        if (o is null) return null;

        return new OrderReadDto(
            o.OrderId, o.CustId, o.EmpId, o.ShipperId,
            o.OrderDate, o.RequiredDate, o.ShippedDate,
            o.Freight, o.ShipName, o.ShipAddress, o.ShipCity, o.ShipCountry,
            [.. o.Details.Select(d => new OrderDetailReadDto(d.ProductId, d.ProductName, d.UnitPrice, d.Qty, d.Discount))]
        );
    }
}
