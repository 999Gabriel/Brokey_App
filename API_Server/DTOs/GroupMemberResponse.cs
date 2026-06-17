namespace API_Server.DTOs;

// Antwort der Gruppen-Mitglieder-Endpoints (GET/POST /api/groups/{id}/members) – ein Gruppenmitglied (UserId, Username, Email, Rolle).
public class GroupMemberResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}
