using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SalesDatePrediction.Domain.Predictions.Ports;
using SalesDatePrediction.Domain.Common.Pagination;
using SalesDatePrediction.Domain.Predictions;

namespace SalesDatePrediction.Infrastructure.Sql;

public sealed class SqlSalesPredictionReadAdapter(IConfiguration configuration) : ISalesPredictionReadPort
{
    private readonly string _connectionString = configuration.GetConnectionString("StoreSample")
           ?? throw new InvalidOperationException("Missing connection string 'StoreSample'.");

    public async Task<IReadOnlyList<CustomerPrediction>> GetPredictionsAsync(CancellationToken ct = default)
    {
        const string sql = @"
;WITH OrdersNorm AS (
    SELECT o.custid, CAST(CAST(o.orderdate AS date) AS datetime) AS orderdate
    FROM Sales.Orders AS o
),
O AS (
    SELECT custid, orderdate,
           LEAD(orderdate) OVER (PARTITION BY custid ORDER BY orderdate) AS next_orderdate
    FROM OrdersNorm
),
Diffs AS (
    SELECT custid, DATEDIFF(DAY, orderdate, next_orderdate) AS DaysBetween
    FROM O WHERE next_orderdate IS NOT NULL
),
AvgPerCustomer AS (
    SELECT custid, CAST(ROUND(AVG(CAST(DaysBetween AS float)),0) AS int) AS AvgDaysBetween
    FROM Diffs GROUP BY custid
),
LastOrder AS (
    SELECT custid, MAX(orderdate) AS LastOrderDate
    FROM OrdersNorm GROUP BY custid
),
GlobalAvg AS (
    SELECT CAST(ROUND(AVG(CAST(DaysBetween AS float)),0) AS int) AS AvgDaysBetween
    FROM Diffs
)
SELECT
    c.companyname AS CustomerName,
    lo.LastOrderDate AS LastOrderDate,
    DATEADD(DAY, COALESCE(apc.AvgDaysBetween, ga.AvgDaysBetween), lo.LastOrderDate) AS NextPredictedOrder
FROM Sales.Customers AS c
JOIN LastOrder AS lo ON lo.custid = c.custid
LEFT JOIN AvgPerCustomer apc ON apc.custid = c.custid
CROSS JOIN GlobalAvg ga
ORDER BY c.companyname;
";
        await using var conn = new SqlConnection(_connectionString);
        var rows = await conn.QueryAsync<CustomerPrediction>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<PaginationResponse<CustomerPrediction>> GetPredictionsPagedAsync(PaginationParams paginationParams, CancellationToken ct = default)
    {
        const string countSql = @"
;WITH OrdersNorm AS (
    SELECT o.custid, CAST(CAST(o.orderdate AS date) AS datetime) AS orderdate
    FROM Sales.Orders AS o
),
LastOrder AS (
    SELECT custid, MAX(orderdate) AS LastOrderDate
    FROM OrdersNorm GROUP BY custid
)
SELECT COUNT(*)
FROM Sales.Customers AS c
JOIN LastOrder AS lo ON lo.custid = c.custid;";

        const string dataSql = @"
;WITH OrdersNorm AS (
    SELECT o.custid, CAST(CAST(o.orderdate AS date) AS datetime) AS orderdate
    FROM Sales.Orders AS o
),
O AS (
    SELECT custid, orderdate,
           LEAD(orderdate) OVER (PARTITION BY custid ORDER BY orderdate) AS next_orderdate
    FROM OrdersNorm
),
Diffs AS (
    SELECT custid, DATEDIFF(DAY, orderdate, next_orderdate) AS DaysBetween
    FROM O WHERE next_orderdate IS NOT NULL
),
AvgPerCustomer AS (
    SELECT custid, CAST(ROUND(AVG(CAST(DaysBetween AS float)),0) AS int) AS AvgDaysBetween
    FROM Diffs GROUP BY custid
),
LastOrder AS (
    SELECT custid, MAX(orderdate) AS LastOrderDate
    FROM OrdersNorm GROUP BY custid
),
GlobalAvg AS (
    SELECT CAST(ROUND(AVG(CAST(DaysBetween AS float)),0) AS int) AS AvgDaysBetween
    FROM Diffs
)
SELECT
    c.companyname AS CustomerName,
    lo.LastOrderDate AS LastOrderDate,
    DATEADD(DAY, COALESCE(apc.AvgDaysBetween, ga.AvgDaysBetween), lo.LastOrderDate) AS NextPredictedOrder
FROM Sales.Customers AS c
JOIN LastOrder AS lo ON lo.custid = c.custid
LEFT JOIN AvgPerCustomer apc ON apc.custid = c.custid
CROSS JOIN GlobalAvg ga
ORDER BY c.companyname
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;";

        await using var conn = new SqlConnection(_connectionString);
        
        var offset = (paginationParams.PageNumber - 1) * paginationParams.PageSize;
        
        var totalRows = await conn.QuerySingleAsync<int>(new CommandDefinition(countSql, cancellationToken: ct));
        var totalPages = (int)Math.Ceiling((double)totalRows / paginationParams.PageSize);
        
        var data = await conn.QueryAsync<CustomerPrediction>(new CommandDefinition(dataSql, new { Offset = offset, paginationParams.PageSize }, cancellationToken: ct));
        
        return new PaginationResponse<CustomerPrediction>
        {
            Data = data.ToList(),
            TotalPages = totalPages,
            TotalRows = totalRows
        };
    }
}
