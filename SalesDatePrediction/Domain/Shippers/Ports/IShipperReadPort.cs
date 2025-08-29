using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Domain.Shippers.Ports;

public interface IShipperReadPort
{
    Task<IReadOnlyList<Shipper>> GetAllAsync(CancellationToken ct = default);
    Task<PaginationResponse<Shipper>> GetPagedAsync(PaginationParams paginationParams, CancellationToken ct = default);
}
