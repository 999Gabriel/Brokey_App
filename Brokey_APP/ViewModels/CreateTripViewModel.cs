using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel der "Trip erstellen"-Seite. Erreichbar vom Home- und vom Trips-Screen.
// Sammelt Name, Beschreibung, Währung und Zeitraum und legt damit über die API eine neue Reise an.
public partial class CreateTripViewModel : BaseViewModel
{
    // Per DI eingespeister Trip-Service für die HTTP-Aufrufe.
    private readonly ITripService _tripService;

    // Eingabe-Felder, an die Formular-Controls im XAML gebunden.
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    // Standardwährung des Trips (Vorauswahl EUR), gebunden an einen Picker.
    [ObservableProperty]
    private string _baseCurrency = "EUR";

    // Start- und Enddatum, gebunden an DatePicker. Vorbelegt: heute bis heute + 3 Tage.
    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(3);

    // Fehlertext + Sichtbarkeits-Flag für die Fehleranzeige.
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    // Auswahlliste der Währungen für den Picker.
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

    // RelayCommand für den Speichern-Button. Validierung:
    // 1) Name darf nicht leer sein. 2) Enddatum darf nicht vor dem Startdatum liegen.
    // Danach baut es ein CreateTripRequest-DTO und ruft TripService.CreateTripAsync (POST /api/trips).
    // Bei Erfolg: eine Ebene zurück und direkt in die Detailseite des neu erstellten Trips navigieren.
    [RelayCommand]
    private async Task SaveTripAsync()
    {
        // Prüfung 1: Trip-Name ist Pflicht.
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Please enter a trip name.";
            HasError = true;
            return;
        }

        // Prüfung 2: Zeitraum muss logisch sein (Ende >= Start).
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
            // DTO zusammenbauen und an die API senden; leere Beschreibung wird als null gesendet.
            var createdTrip = await _tripService.CreateTripAsync(new CreateTripRequest
            {
                Name = Name.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                BaseCurrency = BaseCurrency,
                StartDate = StartDate,
                EndDate = EndDate
            });

            // ".." entfernt diese Create-Seite vom Navigationsstapel, danach direkt in die Detailseite
            // des frisch angelegten Trips (mit dessen Id als Query-Parameter).
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

    // RelayCommand für Abbrechen: navigiert eine Ebene zurück (".."), ohne einen Trip anzulegen.
    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
