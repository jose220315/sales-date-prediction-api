using MediatR;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Application.Employees;

public sealed record EmployeeDto(int EmpId, string FullName);
public sealed record GetEmployeesQuery(PaginationParams? PaginationParams = null) 
    : IRequest<PaginationResponse<EmployeeDto>>;
