namespace Models;

// Zuordnung Land -> Währungscode (Tabelle "CountryCurrencies").
// Vorgeseedete Nachschlagetabelle mit 25 festen Einträgen; hilft, beim Reiseziel automatisch die passende Währung vorzuschlagen.
public class CountryCurrency
{
    public int Id { get; set; } // Primärschlüssel des Eintrags.
    public string Country { get; set; } = string.Empty; // Name des Landes, z.B. "Spain".
    public string CurrencyCode { get; set; } = string.Empty; // Zugehöriger Währungscode (ISO), z.B. "EUR".
}
