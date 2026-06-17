namespace Brokey_APP.Models;

// Vollständige Ausgabe (Antwort von GET/POST/PUT .../expenses und der Recent-Activities). Genutzt in der
// Ausgabenliste der Gruppe, der Ausgaben-Detailseite und der Aktivitätsliste der Startseite.
public class ExpenseResponse
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;       // Name der Gruppe (fertig zur Anzeige).
    public string Currency { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;    // Name der Kategorie (fertig zur Anzeige).
    public int PaidByUserId { get; set; }
    public string PaidByUsername { get; set; } = string.Empty;  // Wer bezahlt hat (fertig zur Anzeige).
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal? Latitude { get; set; }                      // Optionaler Standort (null, wenn keiner erfasst).
    public decimal? Longitude { get; set; }
    public DateTime ExpenseDate { get; set; }                   // Datum der Ausgabe.
    public DateTime CreatedAt { get; set; }                     // Zeitpunkt der Erfassung im System.
    public List<ExpenseSplitResponse> Splits { get; set; } = [];   // Wie sich der Betrag auf die Mitglieder verteilt.

    // Berechnete Hilfs-Property (kein API-Feld): true, wenn beide Koordinaten gesetzt sind.
    // Wird im XAML genutzt, um Karten-/Standortelemente nur bei vorhandenem Standort anzuzeigen.
    public bool HasLocation => Latitude.HasValue && Longitude.HasValue;
}
