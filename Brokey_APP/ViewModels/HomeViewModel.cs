using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel des Home-/Dashboard-Screens (Start-Tab der AppShell nach dem Login).
// Begrüßt den User mit Namen, zeigt die zuletzt erfassten Ausgaben und bietet Schnellzugriffe
// zu den Trips und zum Erstellen eines Trips.
public partial class HomeViewModel : BaseViewModel
{
    // Per DI eingespeiste Services: Auth (User-Infos) und Trip (Ausgaben/Reise-Daten).
    private readonly IAuthService _authService;
    private readonly ITripService _tripService;

    // Anzeigename des Users für die Begrüßung ("Hi, <UserName>"). Standard "Traveler" bis geladen.
    [ObservableProperty]
    private string _userName = "Traveler";

    // Liste der neuesten Ausgaben über alle Trips hinweg; an eine CollectionView im XAML gebunden.
    public ObservableCollection<ExpenseResponse> RecentActivities { get; } = [];

    // Flag: true, wenn es mindestens eine Aktivität gibt. Steuert in der UI, ob die Liste oder
    // ein "Noch keine Aktivitäten"-Platzhalter angezeigt wird.
    [ObservableProperty]
    private bool _hasActivities;

    public HomeViewModel(IAuthService authService, ITripService tripService)
    {
        _authService = authService;
        _tripService = tripService;
        Title = "Home";
        // Konstruktor startet das Laden im Hintergrund (Fire-and-forget mit "_ ="), damit der
        // Screen sofort angezeigt wird und sich danach mit Daten befüllt.
        _ = InitializeAsync();
    }

    // Wird beim Erstellen des ViewModels gestartet: lädt nacheinander User-Infos und Aktivitäten.
    private async Task InitializeAsync()
    {
        await LoadUserAsync();
        await LoadActivitiesAsync();
    }

    // Holt den eingeloggten User über AuthService.GetCurrentUserAsync (GET /api/auth/me) und
    // setzt UserName für die Begrüßung. Bei Fehler (z.B. offline) wird auf "Traveler" zurückgefallen.
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

    // RelayCommand (auch für Pull-to-Refresh nutzbar): ruft TripService.GetRecentActivitiesAsync
    // auf (GET der neuesten Ausgaben über die API), leert die Collection und füllt sie neu.
    // Setzt HasActivities je nachdem, ob Einträge vorhanden sind. Fehler werden still ignoriert.
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

    // RelayCommand: Springt zum Trips-Tab (absolute AppShell-Route "//trips").
    [RelayCommand]
    private async Task OpenTripsAsync()
    {
        await Shell.Current.GoToAsync("//trips");
    }

    // RelayCommand: Öffnet die "Trip erstellen"-Seite (registrierte Route "create-trip").
    [RelayCommand]
    private async Task OpenCreateTripAsync()
    {
        await Shell.Current.GoToAsync("create-trip");
    }

    // RelayCommand: Öffnet die Detailseite einer angetippten Aktivität/Ausgabe.
    // Parameter "expense" kommt aus der getippten Listenzeile. groupId und expenseId werden als
    // Query-Parameter an die Route "expense-detail" übergeben; bei null passiert nichts.
    [RelayCommand]
    private async Task OpenExpenseDetailAsync(ExpenseResponse? expense)
    {
        if (expense == null) return;
        await Shell.Current.GoToAsync($"expense-detail?groupId={expense.GroupId}&expenseId={expense.Id}");
    }
}
