using MediatR;
using SalesDatePrediction.Domain.Products.Ports;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Application.Products;

public sealed class GetProductsHandler(IProductReadPort port)
        : IRequestHandler<GetProductsQuery, PaginationResponse<ProductDto>>
{
    public async Task<PaginationResponse<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        if (request.PaginationParams != null)
        {
            var pagedResult = await port.GetPagedAsync(request.PaginationParams, ct);
            return new PaginationResponse<ProductDto>
            {
                Data = pagedResult.Data.Select(p => new ProductDto(p.ProductId, p.ProductName)).ToList(),
                TotalPages = pagedResult.TotalPages,
                TotalRows = pagedResult.TotalRows
            };
        }
        else
        {
            var allProducts = await port.GetAllAsync(ct);
            return new PaginationResponse<ProductDto>
            {
                Data = allProducts.Select(p => new ProductDto(p.ProductId, p.ProductName)).ToList(),
                TotalPages = 1,
                TotalRows = allProducts.Count
            };
        }
    }
}
