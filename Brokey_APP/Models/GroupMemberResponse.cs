namespace Brokey_APP.Models;

// Ein Mitglied einer Gruppe (Antwort von GET/POST .../members). Angezeigt auf der Gruppen-Detailseite
// und in der Personenauswahl beim Anlegen einer Ausgabe.
public class GroupMemberResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;   // Rolle in der Gruppe (z. B. "Member").
    public DateTime JoinedAt { get; set; }
}
