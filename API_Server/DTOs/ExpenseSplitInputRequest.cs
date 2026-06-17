namespace API_Server.DTOs;

// Teil des CreateExpenseRequest (POST/PUT .../expenses) – eine Split-Eingabe pro User (Prozent oder Betrag).
public class ExpenseSplitInputRequest
{
    public int UserId { get; set; }
    public decimal Value { get; set; }
}
