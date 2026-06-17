using CommunityToolkit.Mvvm.ComponentModel;

namespace Brokey_APP.ViewModels;

// Kleines Hilfs-ViewModel für GENAU EINE Zeile in der Ausgaben-Aufteilung (Split).
// Repräsentiert ein Gruppenmitglied in der Split-Liste von AddExpenseViewModel und bindet pro Zeile
// an die UI: ob das Mitglied beteiligt ist (IsSelected, Checkbox) und dessen Wert (ValueText, Prozent
// oder Betrag). Erbt von ObservableObject, damit Änderungen sofort in der UI sichtbar werden.
public partial class ExpenseSplitMemberInput : ObservableObject
{
    // Unveränderliche Stammdaten des Mitglieds (nur Anzeige / Identifikation).
    public int UserId { get; }
    public string Username { get; }
    public string Role { get; }
    // Berechnet: true, wenn die Rolle "Admin" ist (z.B. für eine Markierung in der Liste).
    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);

    // Veränderbare, an die UI gebundene Felder:
    // IsSelected = ist dieses Mitglied an der Ausgabe beteiligt? (Checkbox)
    // ValueText  = eingegebener Prozent- bzw. Betragswert (nur bei custom Split relevant).
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private string _valueText = string.Empty;

    public ExpenseSplitMemberInput(int userId, string username, string role, bool isSelected = true)
    {
        UserId = userId;
        Username = username;
        Role = role;
        IsSelected = isSelected;
    }
}
