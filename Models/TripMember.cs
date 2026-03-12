namespace Models;

public class TripMember
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public string Role { get; set; } = "Member";

    // Navigation properties
    public Trip Trip { get; set; } = null!;
    public User User { get; set; } = null!;
}
