using MediatR;
using SalesDatePrediction.Domain.Shippers.Ports;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Application.Shippers;

public sealed class GetShippersHandler(IShipperReadPort port)
        : IRequestHandler<GetShippersQuery, PaginationResponse<ShipperDto>>
{
    public async Task<PaginationResponse<ShipperDto>> Handle(GetShippersQuery request, CancellationToken ct)
    {
        if (request.PaginationParams != null)
        {
            var pagedResult = await port.GetPagedAsync(request.PaginationParams, ct);
            return new PaginationResponse<ShipperDto>
            {
                Data = pagedResult.Data.Select(s => new ShipperDto(s.ShipperId, s.CompanyName)).ToList(),
                TotalPages = pagedResult.TotalPages,
                TotalRows = pagedResult.TotalRows
            };
        }
        else
        {
            var allShippers = await port.GetAllAsync(ct);
            return new PaginationResponse<ShipperDto>
            {
                Data = allShippers.Select(s => new ShipperDto(s.ShipperId, s.CompanyName)).ToList(),
                TotalPages = 1,
                TotalRows = allShippers.Count
            };
        }
    }
}
