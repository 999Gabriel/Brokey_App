using System.ComponentModel.DataAnnotations;

namespace API_Server.DTOs;

public class CreateExpenseRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0.01, 100000000)]
    public decimal Amount { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int? PaidByUserId { get; set; }

    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    [StringLength(20)]
    public string SplitMode { get; set; } = "Equal";

    public List<int>? SplitUserIds { get; set; }
    public List<ExpenseSplitInputRequest>? SplitAllocations { get; set; }
}
