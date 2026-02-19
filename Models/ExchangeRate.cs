namespace Models;

public class ExchangeRate
{
    public int Id { get; set; }
    public string FromCurrency { get; set; } = string.Empty; // e.g. "USD"
    public string ToCurrency { get; set; } = string.Empty; // e.g. "EUR"
    public decimal Rate { get; set; } // e.g. 0.92
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}