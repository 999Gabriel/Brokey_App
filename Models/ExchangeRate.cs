namespace Models;

// Wechselkurs zwischen zwei Währungen (Tabelle "ExchangeRates").
// Wird für die Mehrwährungs-Umrechnung genutzt: Beträge in einer Währung werden in eine andere umgerechnet.
public class ExchangeRate
{
    public int Id { get; set; } // Primärschlüssel des Kurs-Eintrags.
    public string FromCurrency { get; set; } = string.Empty; // Ausgangswährung (Währungscode), z.B. "USD".
    public string ToCurrency { get; set; } = string.Empty; // Zielwährung (Währungscode), z.B. "EUR".
    public decimal Rate { get; set; } // Umrechnungsfaktor: 1 FromCurrency = Rate * ToCurrency, z.B. 0.92.
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow; // Zeitpunkt, zu dem der Kurs geholt/aktualisiert wurde (Weltzeit/UTC).
}
