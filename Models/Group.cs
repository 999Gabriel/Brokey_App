namespace Models;

// Untergruppe innerhalb eines Trips (Tabelle "Groups"), z.B. "Hotelzimmer 1" oder "Abendessen".
// Eine Gruppe bündelt eigene Mitglieder (GroupMembers) und eigene Ausgaben.
public class Group
{
    public int Id { get; set; } // Primärschlüssel der Gruppe.
    public string Name { get; set; } = string.Empty; // Name der Gruppe.
    public int TripId { get; set; } // Fremdschlüssel (FK) auf den übergeordneten Trip, zu dem die Gruppe gehört.
    public int CreatedById { get; set; } // Fremdschlüssel (FK) auf den User, der die Gruppe erstellt hat.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Erstellungszeitpunkt der Gruppe (Weltzeit/UTC).

    // ---- Navigation Properties ----
    public Trip Trip { get; set; } = null!; // Navigation (n:1): der übergeordnete Trip zum Fremdschlüssel TripId.
    public User CreatedBy { get; set; } = null!; // Navigation (n:1): der erstellende User zum Fremdschlüssel CreatedById.
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>(); // Navigation (1:n): Mitglieder der Gruppe über die Join-Tabelle GroupMember.
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>(); // Navigation (1:n): alle Ausgaben, die dieser Gruppe zugeordnet sind.
}
