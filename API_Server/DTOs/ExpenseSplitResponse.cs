namespace API_Server.DTOs;

public class ExpenseSplitResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PaidToUserId { get; set; }
    public string PaidToUsername { get; set; } = string.Empty;
    public decimal OwesAmount { get; set; }
    public bool IsPaidByUser { get; set; }
    public bool IsSettled { get; set; }
    public DateTime? SettledAt { get; set; }
}
