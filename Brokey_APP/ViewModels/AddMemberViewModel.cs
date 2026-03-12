using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class AddMemberViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ITripService _tripService;

    [ObservableProperty]
    private int _groupId;

    [ObservableProperty]
    private string _groupName = string.Empty;

    [ObservableProperty]
    private string _usernameOrEmail = string.Empty;

    [ObservableProperty]
    private string _selectedRole = "Member";

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public List<string> Roles { get; } = ["Member", "Admin"];

    public AddMemberViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Add Member";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("groupId", out var rawGroupId) &&
            int.TryParse(rawGroupId?.ToString(), out var groupId))
        {
            GroupId = groupId;
        }

        if (query.TryGetValue("groupName", out var rawGroupName))
        {
            GroupName = Uri.UnescapeDataString(rawGroupName?.ToString() ?? string.Empty);
        }
    }

    [RelayCommand]
    private async Task AddMemberAsync()
    {
        if (GroupId <= 0 || string.IsNullOrWhiteSpace(UsernameOrEmail))
        {
            ErrorMessage = "Enter a username or email.";
            HasError = true;
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            await _tripService.AddGroupMemberAsync(GroupId, new AddGroupMemberRequest
            {
                UsernameOrEmail = UsernameOrEmail.Trim(),
                Role = SelectedRole
            });

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
