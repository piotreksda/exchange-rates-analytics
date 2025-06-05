using System.Globalization;
using ExchangeRates.Dtos;
using ExchangeRates.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRates.ChartQuery;

public class ChartQueryHandler(AppDbContext dbContext) : IRequestHandler<ChartQuery, ChartDto[]>
{
    public async Task<ChartDto[]> Handle(ChartQuery request, CancellationToken cancellationToken)
    {
        var tocurrenciesSqlParam = string.Join(", ", request.To.Select(x => $"'{x}'").ToArray());

        var sql = $"""
                   WITH vars AS (SELECT '{request.From}'::text AS from_currency, 
                       ARRAY[{tocurrenciesSqlParam}]::text[] AS to_currencies,
                       '{request.StartDate:yyyy-MM-dd}':: timestamp as fromDate,
                       '{request.EndDate:yyyy-MM-dd}':: timestamp as endDate
                     ),
                   base_data AS (
                   SELECT
                       "CurrencyCode" AS currencycode,
                       DATE_TRUNC('{request.TimeWindow.ToString().ToLower()}', "EffectiveDate") AS timewindow,
                       AVG("Rate") AS avg_rate
                   FROM public."ExchangeRates", vars
                   WHERE
                       "EffectiveDate" between vars.fromDate and vars.endDate
                     AND (
                       "CurrencyCode" = vars.from_currency
                      OR "CurrencyCode" = ANY(vars.to_currencies)
                       )
                   GROUP BY "CurrencyCode", timewindow

                   UNION ALL

                   SELECT
                       'PLN' AS currencycode,
                       q.timewindow,
                       1.0 AS avg_rate
                   FROM (
                       SELECT DISTINCT DATE_TRUNC('{request.TimeWindow.ToString().ToLower()}', "EffectiveDate") AS timewindow
                       FROM public."ExchangeRates", vars
                       WHERE "EffectiveDate" BETWEEN vars.fromDate AND vars.endDate --'2010-01-01'
                       ) q, vars
                   WHERE
                       vars.from_currency = 'PLN'
                      OR 'PLN' = ANY(vars.to_currencies)
                       ),
                       timewindows AS (
                   SELECT DISTINCT timewindow FROM base_data
                       ),
                       pln_rows AS (
                   SELECT
                       'PLN' AS currencycode,
                       timewindow,
                       1.0 AS avg_rate
                   FROM timewindows
                       ),
                       pivoted AS (
                   SELECT
                       bd_from.timewindow,
                       bd_from.avg_rate AS from_avg_rate,
                       bd_to.currencycode AS to_currency,
                       bd_to.avg_rate AS to_avg_rate
                   FROM base_data bd_from
                       JOIN base_data bd_to
                   ON bd_from.timewindow = bd_to.timewindow
                       JOIN vars
                       ON bd_from.currencycode = vars.from_currency
                       AND bd_to.currencycode = ANY(vars.to_currencies)
                       ),
                       converted AS (
                   SELECT
                       timewindow,
                       vars.from_currency AS currencycode,
                       to_currency,
                       ROUND(from_avg_rate / to_avg_rate, 6) AS avg_rate
                   FROM pivoted, vars
                       )
                   SELECT
                       to_currency AS currencycode,
                       timewindow,
                       avg_rate as AvgRate,
                       1/avg_rate as RevertedAvgRate
                   FROM converted
                   ORDER BY timewindow, currencycode;
                   """;

        var result = await dbContext.Database.SqlQueryRaw<ConvertedExchangeRateDto>(sql)
            .ToListAsync(cancellationToken: cancellationToken);

        return
            result.GroupBy(x => x.TimeWindow).Select(group => new ChartDto()
            {
                Label = ToLabel(group.Key, request.TimeWindow),
                Data = group.ToDictionary(
                    x => x.CurrencyCode,
                    x => request.ChartMode == ChartMode.Buy ? x.AvgRate : x.RevertedAvgRate)
            }).ToArray();
    }
    
    private static string ToLabel(DateTime date, ChartTimeWindow timeWindow)
    {
        return timeWindow switch
        {
            ChartTimeWindow.Day => date.ToString("yyyy-MM-dd"),
            ChartTimeWindow.Quarter => $"Q{(date.Month - 1) / 3 + 1} {date.Year}",
            ChartTimeWindow.Month => date.ToString("yyyy-MM"),
            ChartTimeWindow.Year => date.ToString("yyyy"),
            _ => throw new ArgumentOutOfRangeException(nameof(timeWindow), timeWindow, null)
        };
    }
}