namespace Brokey_APP.Models;

public class ExpenseSplitResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsSettled { get; set; }
}
