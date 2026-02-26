namespace Models;

public class ExpenseSplit
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public bool IsSettled { get; set; } = false;
    public DateTime? SettledAt { get; set; }

    // Navigation properties
    public Expense Expense { get; set; } = null!;
    public User User { get; set; } = null!;
}