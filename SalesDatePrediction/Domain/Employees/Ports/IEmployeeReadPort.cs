namespace SalesDatePrediction.Domain.Employees.Ports;

public interface IEmployeeReadPort
{
    Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default);
}
