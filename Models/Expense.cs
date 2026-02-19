namespace Models;

public class Expense
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int PaidByUserId { get; set; } // Who actually paid
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty; // e.g. "Dinner at La Boqueria"
    public string? Description { get; set; }
    public decimal Amount { get; set; } // In the trip's currency
    public decimal? Latitude { get; set; } // Map pin
    public decimal? Longitude { get; set; } // Map pin
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}