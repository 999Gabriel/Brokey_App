using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class CreateTripViewModel : BaseViewModel
{
    private readonly ITripService _tripService;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _baseCurrency = "EUR";

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(3);

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public List<string> Currencies { get; } =
    [
        "EUR", "USD", "GBP", "CHF", "JPY", "AUD", "CAD", "TRY", "THB", "MXN",
        "BRL", "CZK", "HUF", "PLN", "SEK", "NOK", "DKK"
    ];

    public CreateTripViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Create Trip";
    }

    [RelayCommand]
    private async Task SaveTripAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Please enter a trip name.";
            HasError = true;
            return;
        }

        if (EndDate.Date < StartDate.Date)
        {
            ErrorMessage = "End date must be on or after the start date.";
            HasError = true;
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            var createdTrip = await _tripService.CreateTripAsync(new CreateTripRequest
            {
                Name = Name.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                BaseCurrency = BaseCurrency,
                StartDate = StartDate,
                EndDate = EndDate
            });

            await Shell.Current.GoToAsync("..");
            await Shell.Current.GoToAsync($"trip-detail?tripId={createdTrip.Id}");
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
