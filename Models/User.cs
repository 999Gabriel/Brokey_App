namespace Models;

// Registrierter Benutzer der App. Diese Klasse wird per EF Core auf die Tabelle "Users" in MySQL gemappt.
// Der User ist der zentrale Anker: An ihm hängen Login/Authentifizierung und alle Beziehungen zu Trips, Gruppen und Ausgaben.
public class User
{
    public int Id { get; set; } // Primärschlüssel (Primary Key); wird von der Datenbank automatisch hochgezählt.
    public string Username { get; set; } = string.Empty; // Anzeigename des Benutzers. Standardwert leerer String, damit das Feld nie null ist.
    public string Email { get; set; } = string.Empty; // E-Mail-Adresse, dient als eindeutige Login-Kennung.
    public string PasswordHash { get; set; } = string.Empty; // Gehashtes (verschlüsseltes) Passwort. Aus Sicherheitsgründen wird das Passwort NIE im Klartext gespeichert.
    public string BaseCurrency { get; set; } = "EUR"; // Bevorzugte Standardwährung des Users (z.B. für Anzeige/Umrechnung). Voreinstellung: Euro.
    public string? ProfileImagePath { get; set; } // Dateipfad zum Profilbild. Das "?" bedeutet: darf null sein (Bild ist optional).
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Zeitstempel der Registrierung. UtcNow = Weltzeit, damit Zeitzonen keine Rolle spielen.

    // ---- Navigation Properties ----
    // Navigation-Properties verbinden diese Entität mit anderen Tabellen. EF Core lädt darüber die zugehörigen Datensätze.
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>(); // Navigation (1:n): alle Gruppen-Mitgliedschaften dieses Users über die Join-Tabelle GroupMember.
    public ICollection<TripMember> TripMemberships { get; set; } = new List<TripMember>(); // Navigation (1:n): alle Trip-Teilnahmen dieses Users über die Join-Tabelle TripMember.
    public ICollection<Expense> PaidExpenses { get; set; } = new List<Expense>(); // Navigation (1:n): alle Ausgaben, die dieser User bezahlt hat (Gegenstück zu Expense.PaidByUserId).
    public ICollection<ExpenseSplit> ExpenseSplits { get; set; } = new List<ExpenseSplit>(); // Navigation (1:n): alle Kosten-Anteile, die dieser User schuldet (Gegenstück zu ExpenseSplit.UserId).
    public ICollection<Group> CreatedGroups { get; set; } = new List<Group>(); // Navigation (1:n): alle Gruppen, die dieser User selbst erstellt hat (Gegenstück zu Group.CreatedById).
    public ICollection<Trip> CreatedTrips { get; set; } = new List<Trip>(); // Navigation (1:n): alle Trips, die dieser User selbst erstellt hat (Gegenstück zu Trip.CreatedById).
}
