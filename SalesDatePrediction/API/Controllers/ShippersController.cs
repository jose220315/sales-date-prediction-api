using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesDatePrediction.Application.Shippers;

namespace SalesDatePrediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ShippersController (IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ShipperDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ShipperDto>>> Get(CancellationToken ct)
        => Ok(await mediator.Send(new GetShippersQuery(), ct));
}
