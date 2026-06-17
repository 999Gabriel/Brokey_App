namespace Brokey_APP.Models;

// Ein Mitglied eines Trips (Teil von TripDetailResponse.Members), angezeigt auf der Trip-Detailseite.
public class TripMemberResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;   // Rolle im Trip, z. B. "Owner" oder "Member".
    public DateTime JoinedAt { get; set; }             // Beitrittszeitpunkt.
}
