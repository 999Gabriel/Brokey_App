namespace Models;

// Join-Entität (Verknüpfungstabelle "TripMembers"): löst die n:m-Beziehung zwischen User und Trip auf.
// Ein User kann an vielen Trips teilnehmen, und ein Trip kann viele Teilnehmer haben.
// Unique-Constraint auf (TripId, UserId): derselbe User kann demselben Trip nur EINMAL beitreten.
public class TripMember
{
    public int Id { get; set; } // Primärschlüssel der Verknüpfung.
    public int TripId { get; set; } // Fremdschlüssel (FK) auf den Trip, an dem teilgenommen wird.
    public int UserId { get; set; } // Fremdschlüssel (FK) auf den teilnehmenden User.
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow; // Zeitpunkt des Beitritts (Weltzeit/UTC).
    public string Role { get; set; } = "Member"; // Rolle im Trip, z.B. "Owner" (Ersteller) oder "Member" (normales Mitglied).

    // ---- Navigation Properties ----
    public Trip Trip { get; set; } = null!; // Navigation (n:1): das Trip-Objekt zum Fremdschlüssel TripId.
    public User User { get; set; } = null!; // Navigation (n:1): das User-Objekt zum Fremdschlüssel UserId.
}
