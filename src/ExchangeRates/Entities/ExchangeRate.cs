using System.ComponentModel.DataAnnotations;

namespace ExchangeRates.Entities;

public class ExchangeRate
{
    public ExchangeRate()
    {
        
    }
    
    [Key]
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
}