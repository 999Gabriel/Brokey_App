using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class TripSummaryViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ITripService _tripService;

    [ObservableProperty]
    private int _tripId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _baseCurrency = string.Empty;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private int _durationDays;

    [ObservableProperty]
    private decimal _totalExpenseAmount;

    [ObservableProperty]
    private int _memberCount;

    [ObservableProperty]
    private int _groupCount;

    [ObservableProperty]
    private int _expenseCount;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public ObservableCollection<GroupResponse> Groups { get; } = [];
    public ObservableCollection<TripMemberResponse> Members { get; } = [];

    public bool HasGroups => Groups.Count > 0;

    public TripSummaryViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Summary";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("tripId", out var rawTripId) &&
            int.TryParse(rawTripId?.ToString(), out var tripId))
        {
            TripId = tripId;
            _ = LoadAsync();
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (TripId <= 0 || IsBusy) return;

        IsBusy = true;
        HasError = false;

        try
        {
            var trip = await _tripService.GetTripAsync(TripId);

            Title = trip.Name;
            Name = trip.Name;
            Description = trip.Description ?? string.Empty;
            BaseCurrency = trip.BaseCurrency;
            StartDate = trip.StartDate;
            EndDate = trip.EndDate;
            DurationDays = trip.DurationDays;
            TotalExpenseAmount = trip.TotalExpenseAmount;
            MemberCount = trip.MemberCount;
            GroupCount = trip.GroupCount;
            ExpenseCount = trip.ExpenseCount;

            Groups.Clear();
            foreach (var g in trip.Groups)
                Groups.Add(g);

            Members.Clear();
            foreach (var m in trip.Members)
                Members.Add(m);

            OnPropertyChanged(nameof(HasGroups));
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
