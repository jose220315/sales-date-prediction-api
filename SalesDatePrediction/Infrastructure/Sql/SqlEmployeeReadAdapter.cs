using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SalesDatePrediction.Domain.Employees;
using SalesDatePrediction.Domain.Employees.Ports;

namespace SalesDatePrediction.Infrastructure.Sql;

public sealed class SqlEmployeeReadAdapter : IEmployeeReadPort
{
    private readonly string _cs;
    public SqlEmployeeReadAdapter(IConfiguration cfg)
        => _cs = cfg.GetConnectionString("StoreSample")
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
}
