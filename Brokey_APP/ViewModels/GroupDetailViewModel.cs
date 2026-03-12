using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class GroupDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ITripService _tripService;

    public ObservableCollection<GroupMemberResponse> Members { get; } = [];

    [ObservableProperty]
    private int _groupId;

    [ObservableProperty]
    private string _groupName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public GroupDetailViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Group";
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
            Title = GroupName;
        }

        _ = LoadMembersAsync();
    }

    [RelayCommand]
    private async Task LoadMembersAsync()
    {
        if (GroupId <= 0 || IsBusy)
        {
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            var members = await _tripService.GetGroupMembersAsync(GroupId);
            Members.Clear();

            foreach (var member in members)
            {
                Members.Add(member);
            }
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
    private async Task OpenAddMemberAsync()
    {
        await Shell.Current.GoToAsync(
            $"add-member?groupId={GroupId}&groupName={Uri.EscapeDataString(GroupName)}");
    }

    [RelayCommand]
    private async Task RemoveMemberAsync(GroupMemberResponse? member)
    {
        if (member == null)
        {
            return;
        }

        bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
            "Remove Member",
            $"Remove {member.Username} from {GroupName}?",
            "Remove",
            "Cancel");

        if (!confirm)
        {
            return;
        }

        try
        {
            await _tripService.RemoveGroupMemberAsync(GroupId, member.UserId);
            Members.Remove(member);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }
}
