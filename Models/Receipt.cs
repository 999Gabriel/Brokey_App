namespace Models;

public class Receipt
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    public string ImagePath { get; set; } = string.Empty; // URL or file path to the receipt image
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}