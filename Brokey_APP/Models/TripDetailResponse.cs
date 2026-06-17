namespace Brokey_APP.Models;

// Vollständige Trip-Details (Antwort von GET /api/trips/{id} und POST /api/trips). Genutzt auf der
// Trip-Detailseite und der Trip-Zusammenfassung – enthält Kennzahlen sowie die Listen von Mitgliedern und Gruppen.
public class TripDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;   // Name des Erstellers (fertig zur Anzeige).
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DurationDays { get; set; }                 // Vorberechnete Dauer in Tagen.
    public int GroupCount { get; set; }                   // Anzahl Gruppen.
    public int MemberCount { get; set; }                  // Anzahl Mitglieder.
    public int ExpenseCount { get; set; }                 // Anzahl aller Ausgaben im Trip.
    public decimal TotalExpenseAmount { get; set; }       // Summe aller Ausgaben (in BaseCurrency).
    public List<TripMemberResponse> Members { get; set; } = [];   // Alle Trip-Mitglieder.
    public List<GroupResponse> Groups { get; set; } = [];         // Alle Gruppen des Trips.
}
