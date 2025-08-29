using MediatR;
using SalesDatePrediction.Domain.Common.Pagination;
using System.Collections.Generic;

namespace SalesDatePrediction.Application.Products;

public sealed record ProductDto(int ProductId, string ProductName);
public sealed record GetProductsQuery(PaginationParams? PaginationParams = null) 
    : IRequest<PaginationResponse<ProductDto>>;
