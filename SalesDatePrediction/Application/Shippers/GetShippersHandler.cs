using MediatR;
using SalesDatePrediction.Domain.Shippers.Ports;

namespace SalesDatePrediction.Application.Shippers;

public sealed class GetShippersHandler
    : IRequestHandler<GetShippersQuery, IReadOnlyList<ShipperDto>>
{
    private readonly IShipperReadPort _port;
    public GetShippersHandler(IShipperReadPort port) => _port = port;

    public async Task<IReadOnlyList<ShipperDto>> Handle(GetShippersQuery request, CancellationToken ct)
        => (await _port.GetAllAsync(ct))
            .Select(s => new ShipperDto(s.ShipperId, s.CompanyName))
            .ToList();
}
