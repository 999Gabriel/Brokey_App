namespace Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = "EUR";
    public string? ProfileImagePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    public ICollection<TripMember> TripMemberships { get; set; } = new List<TripMember>();
    public ICollection<Expense> PaidExpenses { get; set; } = new List<Expense>();
    public ICollection<ExpenseSplit> ExpenseSplits { get; set; } = new List<ExpenseSplit>();
    public ICollection<Group> CreatedGroups { get; set; } = new List<Group>();
    public ICollection<Trip> CreatedTrips { get; set; } = new List<Trip>();
}
