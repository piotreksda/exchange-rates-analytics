namespace ExchangeRates.ExternalDependencies.Nbp;

public sealed class NbpHttpClient(HttpClient httpClient)
{
    public async Task<NbpRateDto[]> GetRatesFromTo(DateTime from, DateTime to)
    {
        var url = $"api/exchangerates/tables/A/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}";
        var response = await httpClient.GetAsync(url);
        var dbg = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to fetch rates: {response.ReasonPhrase}");
        }

        return await response.Content.ReadFromJsonAsync<NbpRateDto[]>() ?? [];
    }
}