using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel des Trips-Screens (Trips-Tab der AppShell). Listet alle Reisen des Users auf und
// erlaubt das Öffnen eines Trips (Detailseite) sowie das Anlegen eines neuen Trips.
public partial class TripsViewModel : BaseViewModel
{
    // Per DI: TokenStorage (zum Löschen des Tokens bei abgelaufener Session) und Trip-Service (HTTP).
    private readonly ITokenStorageService _tokenStorageService;
    private readonly ITripService _tripService;

    // Liste der Reisen des Users; an eine CollectionView im XAML gebunden.
    public ObservableCollection<TripSummaryResponse> Trips { get; } = [];

    // Text, der angezeigt wird, wenn noch keine Trips existieren.
    [ObservableProperty]
    private string _emptyMessage = "No trips yet. Create your first one to get started.";

    // Fehlertext + Sichtbarkeits-Flag für die Fehleranzeige.
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    // Berechnete Properties für die Sichtbarkeitssteuerung in der UI:
    // HasTrips -> Liste anzeigen; IsEmpty -> Leer-Hinweis anzeigen.
    // Da Trips eine Collection ist, müssen diese nach dem Befüllen manuell per
    // OnPropertyChanged benachrichtigt werden (siehe LoadTripsAsync).
    public bool HasTrips => Trips.Count > 0;
    public bool IsEmpty => Trips.Count == 0;

    public TripsViewModel(ITripService tripService, ITokenStorageService tokenStorageService)
    {
        _tripService = tripService;
        _tokenStorageService = tokenStorageService;
        Title = "My Trips";
    }

    // RelayCommand: Lädt alle Trips des Users über TripService.GetTripsAsync (GET /api/trips),
    // leert die Collection und füllt sie neu. Danach werden HasTrips/IsEmpty aktualisiert.
    // Tritt der spezielle "Session abgelaufen"-Fehler auf (vom AuthHttpMessageHandler bei HTTP 401
    // erzeugt): Token löschen und zurück zur AuthShell (Login) leiten.
    [RelayCommand]
    private async Task LoadTripsAsync()
    {
        // Doppeltes Laden verhindern, wenn bereits ein Ladevorgang läuft.
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

            // Collection-abhängige Properties manuell benachrichtigen, damit die UI Liste/Leer-Hinweis umschaltet.
            OnPropertyChanged(nameof(HasTrips));
            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;

            // Spezialfall abgelaufene Session: Token wegwerfen und User zum Login zwingen.
            if (string.Equals(ex.Message, "Your session is no longer valid. Please log in again.", StringComparison.Ordinal))
            {
                await _tokenStorageService.ClearTokenAsync();
                Application.Current!.Windows[0].Page = new AuthShell();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // RelayCommand: Öffnet die "Trip erstellen"-Seite (Route "create-trip").
    [RelayCommand]
    private async Task OpenCreateTripAsync()
    {
        await Shell.Current.GoToAsync("create-trip");
    }

    // RelayCommand: Öffnet die Detailseite eines angetippten Trips. Der Parameter "trip" stammt aus
    // der getippten Listenzeile; dessen Id wird als Query-Parameter an die Route "trip-detail" übergeben.
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
