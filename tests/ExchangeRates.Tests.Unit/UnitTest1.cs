using System.Net.Http.Json;
using ExchangeRates.Dtos;
using ExchangeRates.Entities;
namespace ExchangeRates.Tests.Unit;

public sealed class UnitTest1(ApiFixture apiFixture) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task When_QuarterTimeWindow_Then_ReturnsCorrectLabels()
    {
        await apiFixture.MigrateData([
            new ExchangeRate()
            {
                CurrencyCode = "USD",
                Rate = 4.5m,
                EffectiveDate = new DateTime(2020, 1, 1, 1, 1, 1, 1, 1).ToUniversalTime(),
                Id = Guid.CreateVersion7()
            },
            new ExchangeRate()
            {
                CurrencyCode = "USD",
                Rate = 4.5m,
                EffectiveDate = new DateTime(2020, 12, 1, 1, 1, 1, 1, 1).ToUniversalTime(),
                Id = Guid.CreateVersion7()
            }
        ]);
        
        var client = apiFixture.CreateClient();
        var response = await client.GetAsync("api/chart?from=PLN&to=USD&chartMode=Sell&chartTimeWindow=Quarter&startDate=2010-01-01&endDate=2025-06-05");
        var result = await response.Content.ReadFromJsonAsync<ChartDto[]>();
        var labels = result?.Select(x => x.Label).ToArray();
        Assert.NotNull(result);
        Assert.Contains("Q1 2020", labels!);
        Assert.Contains("Q4 2020", labels!);
        Assert.Equal(2, result!.Length);
    }

    [Fact]
    public async Task When_MonthTimeWindow_Then_ReturnsCorrectLabels()
    {
        await apiFixture.MigrateData([
            new ExchangeRate()
            {
                CurrencyCode = "USD",
                Rate = 4.5m,
                EffectiveDate = new DateTime(2020, 1, 1, 1, 1, 1, 1, 1).ToUniversalTime(),
                Id = Guid.CreateVersion7()
            },
            new ExchangeRate()
            {
                CurrencyCode = "USD",
                Rate = 4.5m,
                EffectiveDate = new DateTime(2020, 2, 1, 1, 1, 1, 1, 1).ToUniversalTime(),
                Id = Guid.CreateVersion7()
            }
        ]);
        var client = apiFixture.CreateClient();
        var response = await client.GetAsync("api/chart?from=PLN&to=USD&chartMode=Sell&chartTimeWindow=Month&startDate=2010-01-01&endDate=2025-06-05");
        var result = await response.Content.ReadFromJsonAsync<ChartDto[]>();
        var labels = result?.Select(x => x.Label).ToArray();
        Assert.NotNull(result);
        Assert.Contains("2020-01", labels!);
        Assert.Contains("2020-02", labels!);
        Assert.Equal(2, result!.Length);
    }

    [Fact]
    public async Task When_YearTimeWindow_Then_ReturnsCorrectLabels()
    {
        await apiFixture.MigrateData([
            new ExchangeRate()
            {
                CurrencyCode = "USD",
                Rate = 4.5m,
                EffectiveDate = new DateTime(2020, 1, 1, 1, 1, 1, 1, 1).ToUniversalTime(),
                Id = Guid.CreateVersion7()
            },
            new ExchangeRate()
            {
                CurrencyCode = "USD",
                Rate = 4.5m,
                EffectiveDate = new DateTime(2021, 1, 1, 1, 1, 1, 1, 1).ToUniversalTime(),
                Id = Guid.CreateVersion7()
            }
        ]);
        var client = apiFixture.CreateClient();
        var response = await client.GetAsync("api/chart?from=PLN&to=USD&chartMode=Sell&chartTimeWindow=Year&startDate=2010-01-01&endDate=2025-06-05");
        var result = await response.Content.ReadFromJsonAsync<ChartDto[]>();
        var labels = result?.Select(x => x.Label).ToArray();
        Assert.NotNull(result);
        Assert.Contains("2020", labels!);
        Assert.Contains("2021", labels!);
        Assert.Equal(2, result!.Length);
    }
}

