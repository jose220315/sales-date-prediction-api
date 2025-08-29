using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SalesDatePrediction.Domain.Products;
using SalesDatePrediction.Domain.Products.Ports;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Infrastructure.Sql;

public sealed class SqlProductReadAdapter(IConfiguration cfg) : IProductReadPort
{
    private readonly string _cs = cfg.GetConnectionString("StoreSample")
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

    public async Task<PaginationResponse<Product>> GetPagedAsync(PaginationParams paginationParams, CancellationToken ct = default)
    {
        const string countSql = @"SELECT COUNT(*) FROM Production.Products;";
        
        const string dataSql = @"
SELECT productid AS ProductId, productname AS ProductName
FROM Production.Products
ORDER BY ProductName
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;";

        await using var cn = new SqlConnection(_cs);
        
        var offset = (paginationParams.PageNumber - 1) * paginationParams.PageSize;
        
        var totalRows = await cn.QuerySingleAsync<int>(new CommandDefinition(countSql, cancellationToken: ct));
        var totalPages = (int)Math.Ceiling((double)totalRows / paginationParams.PageSize);
        
        var data = await cn.QueryAsync<Product>(new CommandDefinition(dataSql, new { Offset = offset, paginationParams.PageSize }, cancellationToken: ct));
        
        return new PaginationResponse<Product>
        {
            Data = data.ToList(),
            TotalPages = totalPages,
            TotalRows = totalRows
        };
    }
}
