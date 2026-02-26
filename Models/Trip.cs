namespace Models;

public class Trip
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Country { get; set; } = string.Empty;
    public string Currency { get; set; } = "EUR";
    public int? GroupId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Group? Group { get; set; }
    public User CreatedBy { get; set; } = null!;
    public ICollection<TripMember> Members { get; set; } = new List<TripMember>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}