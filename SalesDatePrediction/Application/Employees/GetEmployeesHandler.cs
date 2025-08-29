using MediatR;
using SalesDatePrediction.Domain.Employees.Ports;

namespace SalesDatePrediction.Application.Employees;

public sealed class GetEmployeesHandler:IRequestHandler<GetEmployeesQuery, IReadOnlyList<EmployeeDto>>
{
    private readonly IEmployeeReadPort _port;
    public GetEmployeesHandler(IEmployeeReadPort port) => _port = port;

    public async Task<IReadOnlyList<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken ct)
        => (await _port.GetAllAsync(ct))
            .Select(e => new EmployeeDto(e.EmpId, e.FullName))
            .ToList();
}
