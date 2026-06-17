namespace Models;

// Zentrale Domänen-Entität: eine Reise bzw. ein Ereignis (z.B. "Urlaub Spanien 2026").
// Wird auf die Tabelle "Trips" gemappt und bündelt mehrere Gruppen, Teilnehmer (TripMembers) und Ausgaben.
public class Trip
{
    public int Id { get; set; } // Primärschlüssel des Trips, von der DB automatisch vergeben.
    public string Name { get; set; } = string.Empty; // Name der Reise, der dem User angezeigt wird.
    public string? Description { get; set; } // Optionale Beschreibung ("?" = darf null sein).
    public string BaseCurrency { get; set; } = "EUR"; // Basiswährung des Trips; alle Ausgaben werden zur Auswertung hierauf umgerechnet. Voreinstellung: Euro.
    public int CreatedById { get; set; } // Fremdschlüssel (FK) auf den User, der den Trip erstellt hat – wird automatisch Owner.
    public DateTime StartDate { get; set; } // Geplantes Startdatum der Reise.
    public DateTime EndDate { get; set; } // Geplantes Enddatum der Reise.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Zeitpunkt, an dem der Trip angelegt wurde (Weltzeit/UTC).

    // ---- Navigation Properties ----
    public User CreatedBy { get; set; } = null!; // Navigation (n:1): das User-Objekt zum Fremdschlüssel CreatedById. "null!" sagt dem Compiler: EF füllt das später.
    public ICollection<Group> Groups { get; set; } = new List<Group>(); // Navigation (1:n): alle Gruppen dieses Trips. Beim Löschen des Trips werden sie per Cascade-Delete mitgelöscht.
    public ICollection<TripMember> Members { get; set; } = new List<TripMember>(); // Navigation (1:n): alle Teilnehmer des Trips über die Join-Tabelle TripMember.
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>(); // Navigation (1:n): alle Ausgaben, die zu diesem Trip gehören.
}
