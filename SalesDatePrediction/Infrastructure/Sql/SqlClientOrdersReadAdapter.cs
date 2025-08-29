using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SalesDatePrediction.Domain.Orders;
using SalesDatePrediction.Domain.Orders.Ports;

namespace SalesDatePrediction.Infrastructure.Sql;

public sealed class SqlClientOrdersReadAdapter(IConfiguration cfg) : IClientOrdersReadPort
{
    private readonly string _cs = cfg.GetConnectionString("StoreSample")
            ?? throw new InvalidOperationException("Missing connection string 'StoreSample'.");

    public async Task<IReadOnlyList<ClientOrderSummary>> GetByCustomerAsync(int customerId, CancellationToken ct = default)
    {
        const string sql = @"
SELECT
    o.orderid      AS OrderId,
    o.requireddate AS RequiredDate,
    o.shippeddate  AS ShippedDate,
    o.shipname     AS ShipName,
    o.shipaddress  AS ShipAddress,
    o.shipcity     AS ShipCity
FROM Sales.Orders AS o
WHERE o.custid = @CustomerId
ORDER BY o.orderdate DESC;";

        await using var cn = new SqlConnection(_cs);
        var rows = await cn.QueryAsync<ClientOrderSummary>(
            new CommandDefinition(sql, new { CustomerId = customerId }, cancellationToken: ct));

        return rows.ToList();
    }
}
