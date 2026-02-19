namespace Models;

public class TripMember
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}