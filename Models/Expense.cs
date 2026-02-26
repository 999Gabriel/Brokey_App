namespace Models;

public class Expense
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int PaidByUserId { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Trip Trip { get; set; } = null!;
    public User PaidBy { get; set; } = null!;
    public ExpenseCategory Category { get; set; } = null!;
    public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}