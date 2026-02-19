namespace Models;

public class Trip
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. "Barcelona Summer 2025"
    public string? Description { get; set; }
    public string Country { get; set; } = string.Empty; // e.g. "Spain"
    public string Currency { get; set; } = "EUR"; // Auto-set based on country
    public int? GroupId { get; set; } // Null = solo trip
    public int CreatedByUserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}