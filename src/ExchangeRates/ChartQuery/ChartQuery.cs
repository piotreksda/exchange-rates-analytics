using ExchangeRates.Dtos;
using ExchangeRates.Enums;
using MediatR;

namespace ExchangeRates.ChartQuery;

public sealed record ChartQuery(
    string From,
    string[] To,
    ChartMode ChartMode,
    DateOnly? StartDate,
    DateOnly? EndDate,
    ChartTimeWindow TimeWindow) : IRequest<ChartDto[]>;