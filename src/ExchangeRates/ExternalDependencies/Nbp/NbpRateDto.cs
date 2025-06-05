namespace ExchangeRates.ExternalDependencies.Nbp;

public class NbpRateDto
{
    public string Table { get; set; }
    public string No { get; set; }
    public string EffectiveDate { get; set; }
    public List<NbpRateItemDto> Rates { get; set; }
}

public class NbpRateItemDto
{
    public string Currency { get; set; }
    public string Code { get; set; }
    public decimal Mid { get; set; }
}

public sealed class NbpHttpClientOptions
{
    public string SectionName { get; set; } = "NbpHttpClient";
    public string BaseUrl { get; set; } = "https://api.nbp.pl/";
}
