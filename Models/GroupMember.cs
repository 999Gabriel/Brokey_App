namespace Models;

// Join-Entität (Verknüpfungstabelle "GroupMembers"): löst die n:m-Beziehung zwischen User und Group auf.
// Ein User kann in vielen Gruppen sein, und eine Gruppe kann viele User enthalten.
// Unique-Constraint auf (GroupId, UserId): derselbe User kann derselben Gruppe nur EINMAL beitreten.
public class GroupMember
{
    public int Id { get; set; } // Primärschlüssel der Verknüpfung.
    public int GroupId { get; set; } // Fremdschlüssel (FK) auf die Gruppe.
    public int UserId { get; set; } // Fremdschlüssel (FK) auf den Mitglieds-User.
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow; // Zeitpunkt des Gruppenbeitritts (Weltzeit/UTC).
    public string Role { get; set; } = "Member"; // Rolle in der Gruppe, z.B. "Admin" oder "Member".

    // ---- Navigation Properties ----
    public Group Group { get; set; } = null!; // Navigation (n:1): das Group-Objekt zum Fremdschlüssel GroupId.
    public User User { get; set; } = null!; // Navigation (n:1): das User-Objekt zum Fremdschlüssel UserId.
}
