using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExchangeRates.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRatesStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 CREATE OR REPLACE FUNCTION get_converted_exchange_rates(
                                     from_currency text,
                                     to_currencies text[],
                                     from_date timestamp,
                                     end_date timestamp
                                 )
                                 RETURNS TABLE (
                                     currencycode text,
                                     timewindow timestamp,
                                     avg_rate numeric,
                                     reverted_avg_rate numeric
                                 ) AS $$
                                 BEGIN
                                     RETURN QUERY
                                     WITH vars AS (
                                         SELECT from_currency, to_currencies, from_date AS fromDate, end_date AS endDate
                                     ),
                                     base_data AS (
                                         SELECT
                                             "CurrencyCode" AS currencycode,
                                             DATE_TRUNC('month', "EffectiveDate") AS timewindow,
                                             AVG("Rate") AS avg_rate
                                         FROM public."ExchangeRates", vars
                                         WHERE
                                             "EffectiveDate" BETWEEN vars.fromDate AND vars.endDate
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
                                             SELECT DISTINCT DATE_TRUNC('month', "EffectiveDate") AS timewindow
                                             FROM public."ExchangeRates"
                                             WHERE "EffectiveDate" > '2022-01-01'
                                         ) q, vars
                                         WHERE
                                             vars.from_currency = 'PLN'
                                             OR 'PLN' = ANY(vars.to_currencies)
                                     ),
                                     timewindows AS (
                                         SELECT DISTINCT timewindow FROM base_data
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
                                         avg_rate,
                                         1 / avg_rate AS reverted_avg_rate
                                     FROM converted
                                     ORDER BY timewindow, currencycode;
                                 END;
                                 $$ LANGUAGE plpgsql;
                                 
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
