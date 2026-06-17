namespace Brokey_APP.Models;

// Datencontainer für den Body beim Anlegen ODER Bearbeiten einer Ausgabe (POST/PUT .../expenses),
// befüllt im AddExpenseViewModel. Beschreibt, was wofür von wem bezahlt wurde und wie der Betrag aufgeteilt wird.
public class CreateExpenseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }                          // Gesamtbetrag der Ausgabe.
    public int CategoryId { get; set; }                          // Gewählte Kategorie (siehe ExpenseCategoryResponse).
    public int? PaidByUserId { get; set; }                       // Wer bezahlt hat; null = aktueller Nutzer (Server-Default).
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow; // Datum der Ausgabe, Standard "jetzt" (UTC).
    public decimal? Latitude { get; set; }                       // Optionaler Standort (Breitengrad), null wenn nicht gesetzt.
    public decimal? Longitude { get; set; }                      // Optionaler Standort (Längengrad).
    // Aufteilungs-Modus: "Equal" (gleichmäßig), oder z. B. nach Beträgen/Anteilen über SplitAllocations.
    public string SplitMode { get; set; } = "Equal";
    // Bei gleichmäßiger Teilung: IDs der beteiligten Nutzer (nur diese teilen sich die Kosten).
    public List<int>? SplitUserIds { get; set; }
    // Bei ungleicher Teilung: konkrete Beträge/Anteile pro Nutzer (siehe ExpenseSplitInputRequest).
    public List<ExpenseSplitInputRequest>? SplitAllocations { get; set; }
}
