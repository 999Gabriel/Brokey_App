using System.ComponentModel.DataAnnotations;

namespace API_Server.DTOs;

// Request-Body für POST/PUT /api/groups/{id}/expenses – Eingabedaten zum Anlegen/Aktualisieren einer Ausgabe inkl. Split-Modus.
// Die [Attribute] werden vom [ApiController] automatisch validiert; bei Verstoß antwortet die API mit 400 Bad Request.
public class CreateExpenseRequest
{
    [Required]                              // Titel ist Pflicht
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]                     // optionale Beschreibung, max. 500 Zeichen
    public string? Description { get; set; }

    [Range(0.01, 100000000)]                // Gesamtbetrag der Ausgabe, muss > 0 sein
    public decimal Amount { get; set; }

    [Range(1, int.MaxValue)]                // ID der Ausgabenkategorie (z.B. Food) – muss in der DB existieren
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue)]                // wer hat bezahlt? Optional (null) → Controller setzt Default (Gruppen-Admin/aktueller User)
    public int? PaidByUserId { get; set; }

    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;  // Datum der Ausgabe (Standard: jetzt)

    public decimal? Latitude { get; set; }   // optionale Geo-Koordinaten, wo die Ausgabe entstand
    public decimal? Longitude { get; set; }

    [StringLength(20)]
    public string SplitMode { get; set; } = "Equal";  // "Equal" (gleichmäßig), "Percentage" (Prozent) oder "Amount" (feste Beträge)

    public List<int>? SplitUserIds { get; set; }                       // nur bei "Equal": auf welche User-IDs aufteilen (leer = alle Mitglieder)
    public List<ExpenseSplitInputRequest>? SplitAllocations { get; set; }  // nur bei "Percentage"/"Amount": je User ein Wert (Prozent bzw. Betrag)
}
