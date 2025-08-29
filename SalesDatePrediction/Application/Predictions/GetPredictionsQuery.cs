using MediatR;
using System;
using System.Collections.Generic;

namespace SalesDatePrediction.Application.Predictions;

public sealed record CustomerPredictionDto(
    string CustomerName,
    DateTime LastOrderDate,
    DateTime NextPredictedOrder
);

public sealed record GetPredictionsQuery() : IRequest<IReadOnlyList<CustomerPredictionDto>>;
