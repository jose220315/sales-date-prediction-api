using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SalesDatePrediction.Domain.Shippers;
using SalesDatePrediction.Domain.Shippers.Ports;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Infrastructure.Sql;

public sealed class SqlShipperReadAdapter(IConfiguration cfg) : IShipperReadPort
{
    private readonly string _cs = cfg.GetConnectionString("StoreSample")
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

    public async Task<PaginationResponse<Shipper>> GetPagedAsync(PaginationParams paginationParams, CancellationToken ct = default)
    {
        const string countSql = @"SELECT COUNT(*) FROM Sales.Shippers;";
        
        const string dataSql = @"
SELECT shipperid AS ShipperId, companyname AS CompanyName
FROM Sales.Shippers
ORDER BY CompanyName
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;";

        await using var cn = new SqlConnection(_cs);
        
        var offset = (paginationParams.PageNumber - 1) * paginationParams.PageSize;
        
        var totalRows = await cn.QuerySingleAsync<int>(new CommandDefinition(countSql, cancellationToken: ct));
        var totalPages = (int)Math.Ceiling((double)totalRows / paginationParams.PageSize);
        
        var data = await cn.QueryAsync<Shipper>(new CommandDefinition(dataSql, new { Offset = offset, paginationParams.PageSize }, cancellationToken: ct));
        
        return new PaginationResponse<Shipper>
        {
            Data = data.ToList(),
            TotalPages = totalPages,
            TotalRows = totalRows
        };
    }
}
