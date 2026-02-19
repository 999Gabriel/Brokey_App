namespace Models;

public class ExpenseCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. "Food", "Night Out", "Transport"
    public string? Icon { get; set; } // Icon name or emoji for UI
}