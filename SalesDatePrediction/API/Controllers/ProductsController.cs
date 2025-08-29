using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesDatePrediction.Application.Products;

namespace SalesDatePrediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController (IMediator mediator) : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Get(CancellationToken ct)
        => Ok(await mediator.Send(new GetProductsQuery(), ct));
}
