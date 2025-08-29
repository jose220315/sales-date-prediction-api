using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SalesDatePrediction.Domain.Employees;
using SalesDatePrediction.Domain.Employees.Ports;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Infrastructure.Sql;

public sealed class SqlEmployeeReadAdapter(IConfiguration cfg) : IEmployeeReadPort
{
    private readonly string _cs = cfg.GetConnectionString("StoreSample")
            ?? throw new InvalidOperationException("Missing connection string 'StoreSample'.");

    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = @"
SELECT
    e.empid AS EmpId,
    LTRIM(RTRIM(e.firstname)) + ' ' + LTRIM(RTRIM(e.lastname)) AS FullName
FROM HR.Employees AS e
ORDER BY FullName;";

        await using var conn = new SqlConnection(_cs);
        var rows = await conn.QueryAsync<Employee>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<PaginationResponse<Employee>> GetPagedAsync(PaginationParams paginationParams, CancellationToken ct = default)
    {
        const string countSql = @"SELECT COUNT(*) FROM HR.Employees;";
        
        const string dataSql = @"
SELECT
    e.empid AS EmpId,
    LTRIM(RTRIM(e.firstname)) + ' ' + LTRIM(RTRIM(e.lastname)) AS FullName
FROM HR.Employees AS e
ORDER BY FullName
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;";

        await using var conn = new SqlConnection(_cs);
        
        var offset = (paginationParams.PageNumber - 1) * paginationParams.PageSize;
        
        var totalRows = await conn.QuerySingleAsync<int>(new CommandDefinition(countSql, cancellationToken: ct));
        var totalPages = (int)Math.Ceiling((double)totalRows / paginationParams.PageSize);
        
        var data = await conn.QueryAsync<Employee>(new CommandDefinition(dataSql, new { Offset = offset, paginationParams.PageSize }, cancellationToken: ct));
        
        return new PaginationResponse<Employee>
        {
            Data = data.ToList(),
            TotalPages = totalPages,
            TotalRows = totalRows
        };
    }
}
