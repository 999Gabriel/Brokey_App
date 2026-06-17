namespace Brokey_APP.Models;

// Schlanke Trip-Übersicht (Antwort von GET /api/trips). Pro Trip eine Kachel/Zeile in der Trips-Liste.
public class TripSummaryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public int CreatedById { get; set; }              // ID des Erstellers (zur Anzeige/Rechte-Prüfung).
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int GroupCount { get; set; }               // Vorberechnete Anzahl Gruppen (Badge in der Liste).
    public int MemberCount { get; set; }              // Vorberechnete Anzahl Mitglieder.
}
