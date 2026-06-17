using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel der Trip-Detailseite. Erreichbar vom Trips-/Home-Screen (per tripId).
// Zeigt die Kopfdaten einer Reise (Name, Zeitraum, Währung, Gesamtkosten, Mitglieder) und ihre
// Gruppen, erlaubt das Anlegen neuer Gruppen und den Sprung in die Trip-Zusammenfassung.
// Implementiert IQueryAttributable, um die tripId aus der Navigation zu empfangen.
public partial class TripDetailViewModel : BaseViewModel, IQueryAttributable
{
    // Per DI eingespeister Trip-Service für die HTTP-Aufrufe.
    private readonly ITripService _tripService;

    // Gruppen des Trips (z.B. "Restaurants", "Hotels"); an eine CollectionView gebunden.
    public ObservableCollection<GroupResponse> Groups { get; } = [];

    // Id des aktuell angezeigten Trips, kommt als Query-Parameter über die Navigation rein.
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

    // Summe aller Ausgaben des Trips (in der Basiswährung), wird in der Kopfzeile angezeigt.
    [ObservableProperty]
    private decimal _totalExpenseAmount;

    // Anzahl Mitglieder bzw. Ausgaben des Trips, für die Statistik-Kacheln in der UI.
    [ObservableProperty]
    private int _memberCount;

    [ObservableProperty]
    private int _expenseCount;

    // Mitgliederliste des Trips; an eine CollectionView gebunden.
    [ObservableProperty]
    private ObservableCollection<TripMemberResponse> _members = [];

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public TripDetailViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Trip";
    }

    // IQueryAttributable: Wird von der Shell aufgerufen, sobald zu dieser Seite navigiert wird.
    // Liest den "tripId"-Parameter aus der Route (z.B. von TripsViewModel.OpenTripAsync gesetzt),
    // wandelt ihn in int um und startet bei Erfolg den Ladevorgang.
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("tripId", out var rawTripId) &&
            int.TryParse(rawTripId?.ToString(), out var tripId))
        {
            TripId = tripId;
            _ = LoadTripAsync();
        }
    }

    // RelayCommand: Lädt den Trip von der API und befüllt alle ViewModel-Properties.
    // Bricht ab, wenn keine gültige TripId vorliegt oder bereits geladen wird. Die eigentliche
    // Befüllung erledigt die Hilfsmethode PopulateTripAsync.
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

    // RelayCommand: Öffnet die Trip-Zusammenfassung (Route "trip-summary"), übergibt die aktuelle TripId.
    [RelayCommand]
    private async Task OpenSummaryAsync()
    {
        if (TripId <= 0) return;
        await Shell.Current.GoToAsync($"trip-summary?tripId={TripId}");
    }

    // RelayCommand: Zeigt einen Eingabe-Dialog (DisplayPrompt) für den Gruppennamen. Bei leerer Eingabe
    // Abbruch. Sonst CreateGroupRequest bauen, TripService.CreateGroupAsync aufrufen (POST an die API)
    // und anschließend den Trip neu laden, damit die neue Gruppe in der Liste erscheint.
    [RelayCommand]
    private async Task CreateGroupAsync()
    {
        if (TripId <= 0)
        {
            return;
        }

        var name = await Application.Current!.Windows[0].Page!.DisplayPromptAsync(
            "New Group",
            "What would you like to name this group?",
            accept: "Create",
            placeholder: "e.g. Restaurants, Hotels…",
            maxLength: 50);

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            await _tripService.CreateGroupAsync(TripId, new CreateGroupRequest
            {
                Name = name.Trim()
            });

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

    // RelayCommand: Öffnet die Detailseite einer angetippten Gruppe (Route "group-detail").
    // groupId und groupName werden als Query-Parameter übergeben; der Name wird URL-enkodiert
    // (Uri.EscapeDataString), damit Sonderzeichen/Leerzeichen die URL nicht zerstören.
    [RelayCommand]
    private async Task OpenGroupAsync(GroupResponse? group)
    {
        if (group == null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"group-detail?groupId={group.Id}&groupName={Uri.EscapeDataString(group.Name)}");
    }

    // Hilfsmethode: Holt den Trip über TripService.GetTripAsync (GET /api/trips/{id}) und schreibt
    // alle Felder (Name, Beschreibung, Währung, Datum, Summen) ins ViewModel. Danach werden die
    // Collections Members und Groups geleert und mit den frischen Daten neu befüllt.
    private async Task PopulateTripAsync()
    {
        var trip = await _tripService.GetTripAsync(TripId);
        Title = trip.Name;
        Name = trip.Name;
        Description = trip.Description ?? "No description yet.";
        BaseCurrency = trip.BaseCurrency;
        StartDate = trip.StartDate;
        EndDate = trip.EndDate;
        TotalExpenseAmount = trip.TotalExpenseAmount;
        MemberCount = trip.MemberCount;
        ExpenseCount = trip.ExpenseCount;

        Members.Clear();
        foreach (var member in trip.Members)
        {
            Members.Add(member);
        }

        Groups.Clear();
        foreach (var group in trip.Groups)
        {
            Groups.Add(group);
        }
    }
}
