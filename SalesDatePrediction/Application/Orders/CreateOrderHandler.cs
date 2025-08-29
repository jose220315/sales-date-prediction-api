using MediatR;
using SalesDatePrediction.Domain.Orders;
using SalesDatePrediction.Domain.Orders.Ports;

namespace SalesDatePrediction.Application.Orders;

public sealed class CreateOrderHandler(IOrderWritePort port) : IRequestHandler<CreateOrderCommand, int>
{
    public async Task<int> Handle(CreateOrderCommand req, CancellationToken ct)
    {
        var now = DateTime.Now;
        var orderDate = req.OrderDate ?? now;
        var required = req.RequiredDate ?? orderDate.AddDays(7);

        var order = new CreateOrder(
            req.CustId, req.EmpId, orderDate, required, req.ShippedDate, req.ShipperId,
            req.Freight, req.ShipName, req.ShipAddress, req.ShipCity, req.ShipCountry);

        var detail = new CreateOrderDetail(
            req.Detail.ProductId, req.Detail.UnitPrice, req.Detail.Qty, req.Detail.Discount);

        return await port.AddAsync(order, detail, ct);
    }
}
