using CommunityToolkit.Mvvm.ComponentModel;

namespace Brokey_APP.ViewModels;

public partial class ExpenseSplitMemberInput : ObservableObject
{
    public int UserId { get; }
    public string Username { get; }
    public string Role { get; }
    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private string _valueText = string.Empty;

    public ExpenseSplitMemberInput(int userId, string username, string role, bool isSelected = true)
    {
        UserId = userId;
        Username = username;
        Role = role;
        IsSelected = isSelected;
    }
}
