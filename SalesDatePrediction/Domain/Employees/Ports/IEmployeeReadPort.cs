using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Domain.Employees.Ports;

public interface IEmployeeReadPort
{
    Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default);
    Task<PaginationResponse<Employee>> GetPagedAsync(PaginationParams paginationParams, CancellationToken ct = default);
}
