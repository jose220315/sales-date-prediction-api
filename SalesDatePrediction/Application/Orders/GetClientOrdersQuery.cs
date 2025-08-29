using MediatR;
using System;
using System.Collections.Generic;

namespace SalesDatePrediction.Application.Orders;

public sealed record ClientOrderSummaryDto(
    int OrderId,
    DateTime RequiredDate,
    DateTime? ShippedDate,
    string ShipName,
    string ShipAddress,
    string ShipCity
);

public sealed record GetClientOrdersQuery(int CustomerId)
    : IRequest<IReadOnlyList<ClientOrderSummaryDto>>;
