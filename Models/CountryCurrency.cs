namespace Models;

public class CountryCurrency
{
    public int Id { get; set; }
    public string Country { get; set; } = string.Empty; // e.g. "Spain"
    public string CurrencyCode { get; set; } = string.Empty; // e.g. "EUR"
}