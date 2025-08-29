using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SalesDatePrediction.Domain.Shippers;
using SalesDatePrediction.Domain.Shippers.Ports;

namespace SalesDatePrediction.Infrastructure.Sql;

public sealed class SqlShipperReadAdapter : IShipperReadPort
{
    private readonly string _cs;
    public SqlShipperReadAdapter(IConfiguration cfg)
        => _cs = cfg.GetConnectionString("StoreSample")
            ?? throw new InvalidOperationException("Missing connection string 'StoreSample'.");

    public async Task<IReadOnlyList<Shipper>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = @"
SELECT shipperid AS ShipperId, companyname AS CompanyName
FROM Sales.Shippers
ORDER BY CompanyName;";

        await using var cn = new SqlConnection(_cs);
        var rows = await cn.QueryAsync<Shipper>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToList();
    }
}
