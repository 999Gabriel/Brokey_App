namespace Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AdminUserId { get; set; } // The user who created/manages the group
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}