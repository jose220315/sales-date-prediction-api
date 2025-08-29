using MediatR;
using System;
using System.Collections.Generic;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Application.Predictions;

public sealed record CustomerPredictionDto(
    string CustomerName,
    DateTime LastOrderDate,
    DateTime NextPredictedOrder
);

public sealed record GetPredictionsQuery(PaginationParams? PaginationParams = null) 
    : IRequest<PaginationResponse<CustomerPredictionDto>>;
