using MediatR;
using SalesDatePrediction.Domain.Employees.Ports;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Application.Employees;

public sealed class GetEmployeesHandler(IEmployeeReadPort port) : IRequestHandler<GetEmployeesQuery, PaginationResponse<EmployeeDto>>
{
    public async Task<PaginationResponse<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken ct)
    {
        if (request.PaginationParams != null)
        {
            var pagedResult = await port.GetPagedAsync(request.PaginationParams, ct);
            return new PaginationResponse<EmployeeDto>
            {
                Data = pagedResult.Data.Select(e => new EmployeeDto(e.EmpId, e.FullName)).ToList(),
                TotalPages = pagedResult.TotalPages,
                TotalRows = pagedResult.TotalRows
            };
        }
        else
        {
            var allEmployees = await port.GetAllAsync(ct);
            return new PaginationResponse<EmployeeDto>
            {
                Data = allEmployees.Select(e => new EmployeeDto(e.EmpId, e.FullName)).ToList(),
                TotalPages = 1,
                TotalRows = allEmployees.Count
            };
        }
    }
}
