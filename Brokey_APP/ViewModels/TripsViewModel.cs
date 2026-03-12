using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class TripsViewModel : BaseViewModel
{
    private readonly ITripService _tripService;

    public ObservableCollection<TripSummaryResponse> Trips { get; } = [];

    [ObservableProperty]
    private string _emptyMessage = "No trips yet! Tap + to plan your first adventure.";

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public bool HasTrips => Trips.Count > 0;
    public bool IsEmpty => Trips.Count == 0;

    public TripsViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "My Trips";
    }

    [RelayCommand]
    private async Task LoadTripsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            var trips = await _tripService.GetTripsAsync();
            Trips.Clear();

            foreach (var trip in trips)
            {
                Trips.Add(trip);
            }

            OnPropertyChanged(nameof(HasTrips));
            OnPropertyChanged(nameof(IsEmpty));
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
    private async Task OpenCreateTripAsync()
    {
        await Shell.Current.GoToAsync("create-trip");
    }

    [RelayCommand]
    private async Task OpenTripAsync(TripSummaryResponse? trip)
    {
        if (trip == null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"trip-detail?tripId={trip.Id}");
    }
}
