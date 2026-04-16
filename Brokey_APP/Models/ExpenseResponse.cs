namespace Brokey_APP.Models;

public class ExpenseResponse
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int GroupId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int PaidByUserId { get; set; }
    public string PaidByUsername { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ExpenseSplitResponse> Splits { get; set; } = [];

    public bool HasLocation => Latitude.HasValue && Longitude.HasValue;
}
