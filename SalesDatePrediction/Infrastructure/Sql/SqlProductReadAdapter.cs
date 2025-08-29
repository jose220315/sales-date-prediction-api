using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SalesDatePrediction.Domain.Products;
using SalesDatePrediction.Domain.Products.Ports;

namespace SalesDatePrediction.Infrastructure.Sql;

public sealed class SqlProductReadAdapter : IProductReadPort
{
    private readonly string _cs;
    public SqlProductReadAdapter(IConfiguration cfg)
        => _cs = cfg.GetConnectionString("StoreSample")
            ?? throw new InvalidOperationException("Missing connection string 'StoreSample'.");

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = @"
SELECT productid AS ProductId, productname AS ProductName
FROM Production.Products
ORDER BY ProductName;";

        await using var cn = new SqlConnection(_cs);
        var rows = await cn.QueryAsync<Product>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToList();
    }
}
