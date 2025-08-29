using MediatR;

namespace SalesDatePrediction.Application.Employees;

public sealed record EmployeeDto(int EmpId, string FullName);
public sealed record GetEmployeesQuery() : IRequest<IReadOnlyList<EmployeeDto>>;
