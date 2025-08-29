using MediatR;
using System.Collections.Generic;

namespace SalesDatePrediction.Application.Products;

public sealed record ProductDto(int ProductId, string ProductName);
public sealed record GetProductsQuery() : IRequest<IReadOnlyList<ProductDto>>;
