namespace ExchangeRates.Dtos;

public class ChartDto
{
    public string Label { get; set; } = string.Empty;
    public Dictionary<string, decimal> Data { get; set; } = new();
}

public class ConvertedExchangeRateDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime TimeWindow { get; set; }
    public decimal AvgRate { get; set; }
    public decimal RevertedAvgRate { get; set; }
}