using MediatR;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Application.Shippers;

public sealed record ShipperDto(int ShipperId, string CompanyName);
public sealed record GetShippersQuery(PaginationParams? PaginationParams = null) 
    : IRequest<PaginationResponse<ShipperDto>>;
