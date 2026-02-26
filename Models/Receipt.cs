namespace Models;

public class Receipt
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Expense Expense { get; set; } = null!;
}