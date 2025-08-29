using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesDatePrediction.Application.Orders;
using Swashbuckle.AspNetCore.Annotations;

namespace SalesDatePrediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CustomersController(IMediator mediator) : ControllerBase
{

    [HttpGet("{id:int}/orders")]
    [ProducesResponseType(typeof(IReadOnlyList<ClientOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(int id, CancellationToken ct)
        => Ok(await mediator.Send(new GetClientOrdersQuery(id), ct));
}
