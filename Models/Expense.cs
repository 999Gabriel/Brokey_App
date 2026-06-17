namespace Models;

// Eine einzelne Ausgabe innerhalb eines Trips (Tabelle "Expenses"), z.B. "Tankfüllung 50€".
// Eine Ausgabe wurde von genau einem User bezahlt, gehört zu einer Kategorie und wird über
// ExpenseSplits anteilig auf mehrere User verteilt (wer schuldet wie viel).
public class Expense
{
    public int Id { get; set; } // Primärschlüssel der Ausgabe.
    public int TripId { get; set; } // Fremdschlüssel (FK) auf den Trip, zu dem die Ausgabe gehört.
    public int? GroupId { get; set; } // Fremdschlüssel (FK) auf die Gruppe. Nullable ("?"), da eine Gruppe gelöscht werden kann, ohne die Ausgabe selbst zu löschen.
    public int PaidByUserId { get; set; } // Fremdschlüssel (FK) auf den User, der die Ausgabe ausgelegt/bezahlt hat.
    public int CategoryId { get; set; } // Fremdschlüssel (FK) auf die ExpenseCategory (z.B. Food, Transport).
    public string Title { get; set; } = string.Empty; // Kurzer Titel der Ausgabe (z.B. "Mittagessen").
    public string? Description { get; set; } // Optionale ausführlichere Beschreibung.
    public decimal Amount { get; set; } // Betrag der Ausgabe. decimal wird für Geld verwendet, weil es exakt rechnet (keine Rundungsfehler wie bei double).
    public decimal? Latitude { get; set; } // Optionaler geografischer Breitengrad des Orts, an dem die Ausgabe getätigt wurde.
    public decimal? Longitude { get; set; } // Optionaler geografischer Längengrad des Orts (zusammen mit Latitude die Position auf der Karte).
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow; // Datum, an dem die Ausgabe tatsächlich entstanden ist.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Zeitpunkt, an dem der Eintrag in der App erfasst wurde.

    // ---- Navigation Properties ----
    public Trip Trip { get; set; } = null!; // Navigation (n:1): der Trip zum Fremdschlüssel TripId.
    public Group? Group { get; set; } // Navigation (n:1): die Gruppe zum Fremdschlüssel GroupId. Nullable, falls keine/gelöschte Gruppe.
    public User PaidBy { get; set; } = null!; // Navigation (n:1): der zahlende User zum Fremdschlüssel PaidByUserId.
    public ExpenseCategory Category { get; set; } = null!; // Navigation (n:1): die Kategorie zum Fremdschlüssel CategoryId.
    public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>(); // Navigation (1:n): die Aufteilung der Kosten – ein Split pro beteiligtem User.
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>(); // Navigation (1:n): hochgeladene Belege/Quittungen zu dieser Ausgabe.
}
