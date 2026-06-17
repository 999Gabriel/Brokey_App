using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel der Trip-Zusammenfassungs-Seite. Erreichbar von der Trip-Detailseite (per tripId).
// Zeigt einen kompakten Überblick über eine Reise: Zeitraum/Dauer, Gesamtkosten, Anzahl Mitglieder,
// Gruppen und Ausgaben sowie die Gruppen- und Mitgliederlisten.
// Implementiert IQueryAttributable, um die tripId aus der Navigation zu empfangen.
public partial class TripSummaryViewModel : BaseViewModel, IQueryAttributable
{
    // Per DI eingespeister Trip-Service für die HTTP-Aufrufe.
    private readonly ITripService _tripService;

    // Id des Trips, kommt als Query-Parameter über die Navigation rein.
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

    // Statistik-Werte für die Übersichts-Kacheln: Reisedauer in Tagen, Gesamtkosten, Anzahl
    // Mitglieder, Gruppen und Ausgaben. Werden in LoadAsync aus den API-Daten befüllt.
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

    // Fehlertext + Sichtbarkeits-Flag für die Fehleranzeige.
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    // Gruppen- und Mitgliederlisten der Reise; an CollectionViews im XAML gebunden.
    public ObservableCollection<GroupResponse> Groups { get; } = [];
    public ObservableCollection<TripMemberResponse> Members { get; } = [];

    // Berechnete Property: steuert, ob die Gruppenliste oder ein Leer-Hinweis gezeigt wird.
    // Wird nach dem Befüllen manuell per OnPropertyChanged benachrichtigt (siehe LoadAsync).
    public bool HasGroups => Groups.Count > 0;

    public TripSummaryViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Summary";
    }

    // IQueryAttributable: Liest beim Navigieren die tripId aus der Route (von TripDetailViewModel
    // .OpenSummaryAsync gesetzt) und startet bei Erfolg den Ladevorgang.
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("tripId", out var rawTripId) &&
            int.TryParse(rawTripId?.ToString(), out var tripId))
        {
            TripId = tripId;
            _ = LoadAsync();
        }
    }

    // RelayCommand: Lädt den Trip über TripService.GetTripAsync (GET /api/trips/{id}) und schreibt
    // alle Übersichts-Properties (Name, Zeitraum, Dauer, Gesamtkosten, Zähler) ins ViewModel.
    // Danach werden die Collections Groups und Members neu befüllt und HasGroups benachrichtigt.
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
