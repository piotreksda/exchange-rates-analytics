using System.Globalization;
using ExchangeRates;
using ExchangeRates.ChartQuery;
using ExchangeRates.Entities;
using ExchangeRates.Enums;
using ExchangeRates.ExternalDependencies.Nbp;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddNbpClient(builder.Configuration);
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("pg"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(x => x.Theme = ScalarTheme.Mars);
}

app.MapGet("/", () => "");

app.MapPost("/migrate", async ([FromServices] AppDbContext dbContext, [FromServices] NbpHttpClient nbpClient) =>
{
    var startDate = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime();
    var endDate = DateTime.UtcNow.Date;
    while (startDate <= endDate)
    {
        var startDatePlusTwoDays =
            startDate.AddDays(91) > DateTime.UtcNow.Date ? DateTime.UtcNow : startDate.AddDays(91);
        var results = await nbpClient.GetRatesFromTo(startDate, startDatePlusTwoDays)!;

        foreach (var result in results)
        {
            foreach (var exchangeRate in result.Rates.Select(rate => new ExchangeRate()
                     {
                         CurrencyCode = rate.Code,
                         Rate = rate.Mid,
                         EffectiveDate = DateTime.Parse(result.EffectiveDate).ToUniversalTime()
                     }))
            {
                dbContext.ExchangeRates.Add(exchangeRate);
            }
        }

        dbContext.SaveChanges();
        dbContext.ChangeTracker.Clear();

        startDate = startDate.AddDays(91);
    }
});

app.MapGet("/convert", async ([FromQuery] string from, [FromQuery] string to, [FromQuery] double amount,
    [FromServices] AppDbContext dbContext) =>
{
    var fromRate = from.ToLower() == "pln"
        ? 1.0
        : await dbContext.ExchangeRates
            .Where(x => x.CurrencyCode == from.ToUpper())
            .OrderByDescending(x => x.EffectiveDate)
            .Select(x => (double?)x.Rate)
            .FirstOrDefaultAsync();

    var toRate = to.ToLower() == "pln"
        ? 1.0
        : await dbContext.ExchangeRates
            .Where(x => x.CurrencyCode == to.ToUpper())
            .OrderByDescending(x => x.EffectiveDate)
            .Select(x => (double?)x.Rate)
            .FirstOrDefaultAsync();

    if (fromRate == null || toRate == null)
    {
        return Results.NotFound("Currency not found");
    }

    var convertedAmount = (fromRate.Value / toRate.Value) * amount;

    return Results.Ok(new
    {
        ConvertedAmount = convertedAmount,
        FromCurrency = from.ToUpper(),
        ToCurrency = to.ToUpper()
    });
});

app.MapGet("/chart",
    async ([FromQuery] string from, string[] to, ChartMode chartMode, DateOnly? startDate, DateOnly? endDate,
        ChartTimeWindow chartTimeWindow,
        [FromServices] IMediator mediator) =>
    {
        var query = new ChartQuery(
            from.ToUpper(),
            to.Select(x => x.ToUpper()).ToArray(),
            chartMode,
            startDate,
            endDate,
            chartTimeWindow);
        var result = await mediator.Send(query);
        return Results.Ok(result);
    });

app.UseHttpsRedirection();

await MigrateAsync(app.Services);

app.Run();

return;

static async Task MigrateAsync(IServiceProvider serviceProvider)
{
    await using var scope = serviceProvider.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}