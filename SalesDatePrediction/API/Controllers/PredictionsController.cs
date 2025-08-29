using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesDatePrediction.Application.Predictions;
using SalesDatePrediction.Domain.Common.Pagination;
using Swashbuckle.AspNetCore.Annotations;

namespace SalesDatePrediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PredictionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<CustomerPredictionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginationResponse<CustomerPredictionDto>>> Get(
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
            
        var result = await mediator.Send(new GetPredictionsQuery(paginationParams), ct);
        return Ok(result);
    }
}
