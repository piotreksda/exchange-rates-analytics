using ExchangeRates.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangeRates.Tests.Unit;

public static class TestHelper
{
    public static async Task MigrateData(this ApiFixture apiFixture, List<ExchangeRate> exchangeRates)
    {
        await using var scope = apiFixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await dbContext.ExchangeRates.ExecuteDeleteAsync();
        
        await dbContext.ExchangeRates.AddRangeAsync(exchangeRates);
        
        await dbContext.SaveChangesAsync();
        
        var count = await dbContext.ExchangeRates.CountAsync();
        if (count != exchangeRates.Count)
        {
            throw new InvalidOperationException($"Expected {exchangeRates.Count} records, but found {count}.");
        }
    }
}