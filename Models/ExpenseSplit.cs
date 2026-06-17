namespace Models;

// Anteil eines Users an einer Ausgabe (Tabelle "ExpenseSplits").
// Beispiel: Eine Ausgabe über 60€ wird auf 3 User aufgeteilt -> es entstehen 3 ExpenseSplits zu je 20€.
// Jeder Split sagt: dieser User schuldet diesen Betrag, und ob er ihn schon bezahlt hat.
public class ExpenseSplit
{
    public int Id { get; set; } // Primärschlüssel des Anteils.
    public int ExpenseId { get; set; } // Fremdschlüssel (FK) auf die zugehörige Ausgabe.
    public int UserId { get; set; } // Fremdschlüssel (FK) auf den User, der diesen Anteil schuldet.
    public decimal Amount { get; set; } // Geldbetrag, den dieser User für die Ausgabe schuldet (decimal = exaktes Rechnen mit Geld).
    public bool IsSettled { get; set; } = false; // true = Anteil ist beglichen/bezahlt; false = steht noch offen. Standard: nicht beglichen.
    public DateTime? SettledAt { get; set; } // Zeitpunkt der Begleichung. Nullable, weil bei noch offenen Anteilen kein Datum existiert.

    // ---- Navigation Properties ----
    public Expense Expense { get; set; } = null!; // Navigation (n:1): die Ausgabe zum Fremdschlüssel ExpenseId.
    public User User { get; set; } = null!; // Navigation (n:1): der schuldende User zum Fremdschlüssel UserId.
}
