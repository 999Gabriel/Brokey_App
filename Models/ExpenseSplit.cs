namespace Models;

public class ExpenseSplit
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    public int UserId { get; set; } // Who owes a share
    public decimal Amount { get; set; } // Their equal share
    public bool IsSettled { get; set; } = false; // Marked as paid back
    public DateTime? SettledAt { get; set; }
}