using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class TripDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ITripService _tripService;

    public ObservableCollection<GroupResponse> Groups { get; } = [];

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
    private string _newGroupName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public TripDetailViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Trip";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("tripId", out var rawTripId) &&
            int.TryParse(rawTripId?.ToString(), out var tripId))
        {
            TripId = tripId;
            _ = LoadTripAsync();
        }
    }

    [RelayCommand]
    private async Task LoadTripAsync()
    {
        if (TripId <= 0 || IsBusy)
        {
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            await PopulateTripAsync();
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
    private async Task CreateGroupAsync()
    {
        if (TripId <= 0 || string.IsNullOrWhiteSpace(NewGroupName))
        {
            ErrorMessage = "Please enter a group name.";
            HasError = true;
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            await _tripService.CreateGroupAsync(TripId, new CreateGroupRequest
            {
                Name = NewGroupName.Trim()
            });

            NewGroupName = string.Empty;
            await PopulateTripAsync();
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
    private async Task OpenGroupAsync(GroupResponse? group)
    {
        if (group == null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"group-detail?groupId={group.Id}&groupName={Uri.EscapeDataString(group.Name)}");
    }

    private async Task PopulateTripAsync()
    {
        var trip = await _tripService.GetTripAsync(TripId);
        Title = trip.Name;
        Name = trip.Name;
        Description = trip.Description ?? "No description yet.";
        BaseCurrency = trip.BaseCurrency;
        StartDate = trip.StartDate;
        EndDate = trip.EndDate;

        Groups.Clear();
        foreach (var group in trip.Groups)
        {
            Groups.Add(group);
        }
    }
}
