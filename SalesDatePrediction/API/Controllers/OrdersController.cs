using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesDatePrediction.Application.Orders;

namespace SalesDatePrediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController (IMediator mediator) : ControllerBase
{

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand cmd, CancellationToken ct)
    {
        var orderId = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = orderId }, new { orderId });
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
