using MediatR;
using SalesDatePrediction.Domain.Orders.Ports;

namespace SalesDatePrediction.Application.Orders;

public sealed class GetClientOrdersHandler(IClientOrdersReadPort port)
        : IRequestHandler<GetClientOrdersQuery, IReadOnlyList<ClientOrderSummaryDto>>
{
    public async Task<IReadOnlyList<ClientOrderSummaryDto>> Handle(GetClientOrdersQuery request, CancellationToken ct)
        => (await port.GetByCustomerAsync(request.CustomerId, ct))
           .Select(o => new ClientOrderSummaryDto(o.OrderId, o.RequiredDate, o.ShippedDate, o.ShipName, o.ShipAddress, o.ShipCity))
           .ToList();
}
