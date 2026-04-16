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
    public ObservableCollection<ExpenseResponse> Expenses { get; } = [];
    public ObservableCollection<SettlementTransferResponse> SettlementTransfers { get; } = [];
    public ObservableCollection<SettlementBalanceResponse> SettlementBalances { get; } = [];

    [ObservableProperty]
    private int _groupId;

    [ObservableProperty]
    private string _groupName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public bool HasExpenses => Expenses.Count > 0;
    public bool HasSettlementTransfers => SettlementTransfers.Count > 0;
    public bool IsExpensesEmpty => Expenses.Count == 0;
    public bool IsSettlementEmpty => SettlementTransfers.Count == 0;
    public int ExpensesCount => Expenses.Count;

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
            var expenses = await _tripService.GetGroupExpensesAsync(GroupId);
            var settlement = await _tripService.GetGroupSettlementAsync(GroupId);

            Members.Clear();
            Expenses.Clear();
            SettlementTransfers.Clear();
            SettlementBalances.Clear();

            foreach (var member in members)
            {
                Members.Add(member);
            }

            foreach (var expense in expenses)
            {
                Expenses.Add(expense);
            }

            foreach (var transfer in settlement.Transfers)
            {
                SettlementTransfers.Add(transfer);
            }

            foreach (var balance in settlement.Balances)
            {
                SettlementBalances.Add(balance);
            }

            OnPropertyChanged(nameof(HasExpenses));
            OnPropertyChanged(nameof(HasSettlementTransfers));
            OnPropertyChanged(nameof(IsExpensesEmpty));
            OnPropertyChanged(nameof(IsSettlementEmpty));
            OnPropertyChanged(nameof(ExpensesCount));
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
    private async Task OpenAddExpenseAsync()
    {
        await Shell.Current.GoToAsync(
            $"add-expense?groupId={GroupId}&groupName={Uri.EscapeDataString(GroupName)}");
    }

    [RelayCommand]
    private async Task OpenExpenseDetailAsync(ExpenseResponse? expense)
    {
        if (expense == null) return;
        await Shell.Current.GoToAsync($"expense-detail?groupId={GroupId}&expenseId={expense.Id}");
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
            await LoadMembersAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }
}
