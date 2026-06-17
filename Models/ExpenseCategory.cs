namespace Models;

// Kategorie einer Ausgabe (Tabelle "ExpenseCategories"), z.B. Food, Night Out, Transport.
// Es handelt sich um eine vorgeseedete Nachschlagetabelle mit 8 festen Einträgen (beim Start angelegt).
public class ExpenseCategory
{
    public int Id { get; set; } // Primärschlüssel der Kategorie.
    public string Name { get; set; } = string.Empty; // Anzeigename der Kategorie (z.B. "Transport").
    public string? Icon { get; set; } // Optionales Symbol/Icon-Name für die Anzeige in der App.

    // ---- Navigation Properties ----
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>(); // Navigation (1:n): alle Ausgaben, die dieser Kategorie zugeordnet sind.
}
