using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly ITripService _tripService;

    [ObservableProperty]
    private string _userName = "Traveler";

    public ObservableCollection<ExpenseResponse> RecentActivities { get; } = [];

    [ObservableProperty]
    private bool _hasActivities;

    public HomeViewModel(IAuthService authService, ITripService tripService)
    {
        _authService = authService;
        _tripService = tripService;
        Title = "Home";
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadUserAsync();
        await LoadActivitiesAsync();
    }

    private async Task LoadUserAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            UserName = user.Username;
        }
        catch
        {
            UserName = "Traveler";
        }
    }

    [RelayCommand]
    private async Task LoadActivitiesAsync()
    {
        try
        {
            var activities = await _tripService.GetRecentActivitiesAsync();
            RecentActivities.Clear();
            foreach (var activity in activities)
            {
                RecentActivities.Add(activity);
            }
            HasActivities = RecentActivities.Count > 0;
        }
        catch
        {
            HasActivities = false;
        }
    }

    [RelayCommand]
    private async Task OpenTripsAsync()
    {
        await Shell.Current.GoToAsync("//trips");
    }

    [RelayCommand]
    private async Task OpenCreateTripAsync()
    {
        await Shell.Current.GoToAsync("create-trip");
    }

    [RelayCommand]
    private async Task OpenExpenseDetailAsync(ExpenseResponse? expense)
    {
        if (expense == null) return;
        await Shell.Current.GoToAsync($"expense-detail?groupId={expense.GroupId}&expenseId={expense.Id}");
    }
}
