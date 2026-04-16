using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class ExpenseDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ITripService _tripService;

    [ObservableProperty] private int _groupId;
    [ObservableProperty] private int _expenseId;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private ExpenseResponse? _expense;
    [ObservableProperty] private bool _hasLocation;
    [ObservableProperty] private bool _hasDescription;
    [ObservableProperty] private bool _hasSplits;

    public bool IsNotBusy => !IsBusy;

    public ExpenseDetailViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Expense";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("groupId", out var rawGroupId) &&
            int.TryParse(rawGroupId?.ToString(), out var groupId))
        {
            GroupId = groupId;
        }

        if (query.TryGetValue("expenseId", out var rawExpenseId) &&
            int.TryParse(rawExpenseId?.ToString(), out var expenseId))
        {
            ExpenseId = expenseId;
        }

        _ = LoadExpenseAsync();
    }

    [RelayCommand]
    private async Task LoadExpenseAsync()
    {
        if (GroupId <= 0 || ExpenseId <= 0) return;
        IsBusy = true;
        OnPropertyChanged(nameof(IsNotBusy));
        HasError = false;

        try
        {
            Expense = await _tripService.GetGroupExpenseByIdAsync(GroupId, ExpenseId);
            Title = Expense.Title;
            HasLocation = Expense.HasLocation;
            HasDescription = !string.IsNullOrWhiteSpace(Expense.Description);
            HasSplits = Expense.Splits.Count > 0;
            OnPropertyChanged(nameof(IsNotBusy));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Expense == null) return;

        var confirmed = await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
            "Delete Expense",
            $"Delete \"{Expense.Title}\"? This cannot be undone.",
            "Delete",
            "Cancel");

        if (!confirmed) return;

        IsBusy = true;
        HasError = false;

        try
        {
            await _tripService.DeleteExpenseAsync(GroupId, ExpenseId);
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
}
