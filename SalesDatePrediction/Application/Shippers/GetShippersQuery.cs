using MediatR;

namespace SalesDatePrediction.Application.Shippers;

public sealed record ShipperDto(int ShipperId, string CompanyName);
public sealed record GetShippersQuery() : IRequest<IReadOnlyList<ShipperDto>>;
