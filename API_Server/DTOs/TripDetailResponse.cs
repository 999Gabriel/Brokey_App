namespace API_Server.DTOs;

public class TripDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DurationDays { get; set; }
    public int GroupCount { get; set; }
    public int MemberCount { get; set; }
    public int ExpenseCount { get; set; }
    public decimal TotalExpenseAmount { get; set; }
    public List<TripMemberResponse> Members { get; set; } = [];
    public List<GroupResponse> Groups { get; set; } = [];
}
