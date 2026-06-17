namespace API_Server.DTOs;

// Antwort der Group-Endpoints (POST/GET /api/trips/{id}/groups) – eine Gruppe inkl. Member-/Expense-Anzahl und Gesamtbetrag.
public class GroupResponse
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public int ExpenseCount { get; set; }
    public decimal TotalExpenseAmount { get; set; }
}
