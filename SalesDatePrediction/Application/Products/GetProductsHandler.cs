using MediatR;
using SalesDatePrediction.Domain.Products.Ports;

namespace SalesDatePrediction.Application.Products;

public sealed class GetProductsHandler
    : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductReadPort _port;
    public GetProductsHandler(IProductReadPort port) => _port = port;

    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
        => (await _port.GetAllAsync(ct))
           .Select(p => new ProductDto(p.ProductId, p.ProductName))
           .ToList();
}
