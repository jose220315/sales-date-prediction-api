using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesDatePrediction.Application.Employees;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EmployeesController (IMediator mediator) : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginationResponse<EmployeeDto>>> Get(
        [FromQuery] int? pageNumber, 
        [FromQuery] int? pageSize, 
        CancellationToken ct)
    {
        var paginationParams = (pageNumber.HasValue || pageSize.HasValue) 
            ? new PaginationParams 
              { 
                  PageNumber = pageNumber ?? 1, 
                  PageSize = pageSize ?? 10 
              }
            : null;
            
        return Ok(await mediator.Send(new GetEmployeesQuery(paginationParams), ct));
    }
}
