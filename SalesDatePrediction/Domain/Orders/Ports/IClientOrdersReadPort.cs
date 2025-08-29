using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesDatePrediction.Domain.Orders.Ports;

public interface IClientOrdersReadPort
{
    Task<IReadOnlyList<ClientOrderSummary>> GetByCustomerAsync(int customerId, CancellationToken ct = default);
}
