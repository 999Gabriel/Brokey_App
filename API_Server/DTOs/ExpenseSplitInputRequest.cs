namespace API_Server.DTOs;

public class ExpenseSplitInputRequest
{
    public int UserId { get; set; }
    public decimal Value { get; set; }
}
