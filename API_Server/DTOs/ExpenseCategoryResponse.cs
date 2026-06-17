namespace API_Server.DTOs;

// Antwort von GET /api/groups/{id}/expense-categories – eine vorgeseedete Ausgabenkategorie (Id, Name, Icon).
public class ExpenseCategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
}
