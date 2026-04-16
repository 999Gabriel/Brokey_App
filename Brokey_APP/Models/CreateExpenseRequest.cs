namespace Brokey_APP.Models;

public class CreateExpenseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public int? PaidByUserId { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string SplitMode { get; set; } = "Equal";
    public List<int>? SplitUserIds { get; set; }
    public List<ExpenseSplitInputRequest>? SplitAllocations { get; set; }
}
