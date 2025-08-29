using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesDatePrediction.Application.Predictions;
using Swashbuckle.AspNetCore.Annotations;

namespace SalesDatePrediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PredictionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PredictionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Sales Date Prediction per customer",
        Description = "Devuelve CustomerName, LastOrderDate y NextPredictedOrder (última orden + promedio de días entre órdenes).")]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerPredictionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerPredictionDto>>> Get(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPredictionsQuery(), ct);
        return Ok(result);
    }
}
