namespace Brokey_APP.Models;

// Eine Ausgaben-Kategorie (z. B. Essen, Transport). Antwort von GET .../expense-categories, genutzt zur Auswahl
// im AddExpenseViewModel. Die Kategorien sind serverseitig vorab angelegt (Seed-Daten).
public class ExpenseCategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }   // Optionales Icon (z. B. Emoji/Symbolname) zur Darstellung der Kategorie.
}
