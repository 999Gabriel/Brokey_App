namespace Brokey_APP.Models;

public class GroupResponse
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
}
