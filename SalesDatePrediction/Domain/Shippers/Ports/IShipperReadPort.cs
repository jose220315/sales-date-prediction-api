using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesDatePrediction.Domain.Shippers.Ports;

public interface IShipperReadPort
{
    Task<IReadOnlyList<Shipper>> GetAllAsync(CancellationToken ct = default);
}
