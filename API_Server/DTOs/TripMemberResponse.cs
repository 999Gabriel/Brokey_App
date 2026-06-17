namespace API_Server.DTOs;

// Teil von TripDetailResponse – ein Trip-Mitglied (UserId, Username, Email, Rolle wie Owner/Member).
public class TripMemberResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}
