using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesDatePrediction.Application.Employees;

namespace SalesDatePrediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EmployeesController (IMediator mediator) : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> Get(CancellationToken ct)
        => Ok(await mediator.Send(new GetEmployeesQuery(), ct));
}
