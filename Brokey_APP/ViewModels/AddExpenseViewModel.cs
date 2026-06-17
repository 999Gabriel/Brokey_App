using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel der "Ausgabe hinzufügen / bearbeiten"-Seite. Erreichbar von der Gruppen-Detailseite
// (neue Ausgabe) und von der Ausgaben-Detailseite (Bearbeiten). Dies ist der komplexeste Screen:
// Er erfasst Titel, Betrag, Kategorie, Datum, optionalen Ort und vor allem die AUFTEILUNG (Split)
// der Kosten auf die Gruppenmitglieder – wahlweise gleichmäßig, nach Prozent oder nach festen Beträgen.
// Implementiert IQueryAttributable, um groupId / optionale expenseId / groupName zu empfangen.
public partial class AddExpenseViewModel : BaseViewModel, IQueryAttributable
{
    // Per DI eingespeiste Services: Trip (Ausgaben/Kategorien/Mitglieder) und Auth (aktueller User).
    private readonly ITripService _tripService;
    private readonly IAuthService _authService;
    // Id des eingeloggten Users – wird als "PaidBy" gesetzt und um Admin-Rechte zu prüfen.
    private int _currentUserId;

    // ── Query params ── (kommen über ApplyQueryAttributes herein)
    // GroupId = Zielgruppe; ExpenseId > 0 bedeutet Bearbeiten-Modus; GroupName nur zur Anzeige.
    [ObservableProperty] private int _groupId;
    [ObservableProperty] private int _expenseId;
    [ObservableProperty] private string _groupName = string.Empty;

    // ── Formularfelder ── (an die Eingabe-Controls gebunden). AmountText ist Text, weil der User
    // tippt; er wird beim Speichern in eine Dezimalzahl geparst.
    [ObservableProperty] private string _titleText = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _amountText = string.Empty;
    [ObservableProperty] private DateTime _expenseDate = DateTime.Today;

    // ── Fehleranzeige ──
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;

    // ── Kategorie ── Liste für den Kategorie-Picker + aktuell gewählte Kategorie.
    public ObservableCollection<ExpenseCategoryResponse> Categories { get; } = [];
    [ObservableProperty] private ExpenseCategoryResponse? _selectedCategory;

    // ── Aufteilung (Split) ──
    // SplitMembers = pro Gruppenmitglied eine Eingabe-Zeile (Auswahl + Wert), siehe ExpenseSplitMemberInput.
    // SplitModes   = die drei Aufteilungsarten für den Picker.
    public ObservableCollection<ExpenseSplitMemberInput> SplitMembers { get; } = [];
    public List<string> SplitModes { get; } = ["Equal", "Percentage", "Amount"];

    // SelectedSplitMode    = aktuell gewählte Aufteilungsart ("Equal"/"Percentage"/"Amount").
    // IsCurrentUserAdmin   = ist der eingeloggte User Admin der Gruppe? (nur Admins dürfen custom splitten).
    // ShowSplitValueInputs = blendet die Wert-Eingabefelder ein (nur bei Percentage/Amount nötig).
    // SplitValueHint       = Platzhalter in den Wertfeldern ("%" bzw. "0.00").
    // ShowSplitPreview/SplitPreviewMessage/SplitPreviewColor = Live-Vorschau, ob die Summe stimmt.
    [ObservableProperty] private string _selectedSplitMode = "Equal";
    [ObservableProperty] private bool _isCurrentUserAdmin;
    [ObservableProperty] private bool _showSplitValueInputs;
    [ObservableProperty] private string _splitValueHint = string.Empty;
    [ObservableProperty] private bool _showSplitPreview;
    [ObservableProperty] private string _splitPreviewMessage = string.Empty;
    [ObservableProperty] private Color _splitPreviewColor = Colors.Green;

    // Abgeleitete Properties für die UI: Kehrwert bzw. Berechtigung für die Custom-Split-Eingabe.
    public bool IsNotCurrentUserAdmin => !IsCurrentUserAdmin;
    public bool CanConfigureCustomSplit => IsCurrentUserAdmin;

    // ── Standort (optional) ──
    // AddLocation       = Schalter, ob überhaupt ein Ort erfasst wird.
    // IsLoadingLocation = true während GPS-/Geocoding-Abfrage (zeigt Ladeanzeige).
    // LocationAddress   = vom User getippte Adresse; LocationName = lesbarer Name des gewählten Orts.
    // SelectedLatitude/Longitude = die tatsächlich gespeicherten Koordinaten; HasLocation = liegen Koordinaten vor.
    [ObservableProperty] private bool _addLocation;
    [ObservableProperty] private bool _isLoadingLocation;
    [ObservableProperty] private string _locationError = string.Empty;
    [ObservableProperty] private bool _hasLocationError;
    [ObservableProperty] private string _locationAddress = string.Empty;
    [ObservableProperty] private string _locationName = string.Empty;
    [ObservableProperty] private bool _hasLocation;
    [ObservableProperty] private double? _selectedLatitude;
    [ObservableProperty] private double? _selectedLongitude;

    // Kehrwert von IsLoadingLocation: erlaubt es, Standort-Buttons während des Ladens zu deaktivieren.
    public bool IsNotLoadingLocation => !IsLoadingLocation;

    public AddExpenseViewModel(ITripService tripService, IAuthService authService)
    {
        _tripService = tripService;
        _authService = authService;
        Title = "Add Expense";
    }

    // IQueryAttributable: Liest beim Navigieren groupId, die optionale expenseId und groupName aus der
    // Route. Ist eine expenseId vorhanden, schaltet der Titel auf "Edit Expense" (Bearbeiten-Modus).
    // Danach werden die Anfangsdaten geladen.
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("groupId", out var rawGroupId) &&
            int.TryParse(rawGroupId?.ToString(), out var groupId))
        {
            GroupId = groupId;
        }

        if (query.TryGetValue("expenseId", out var rawExpenseId) &&
            int.TryParse(rawExpenseId?.ToString(), out var expenseId))
        {
            ExpenseId = expenseId;
            Title = "Edit Expense";
        }

        if (query.TryGetValue("groupName", out var rawGroupName))
        {
            GroupName = Uri.UnescapeDataString(rawGroupName?.ToString() ?? string.Empty);
        }

        _ = LoadInitialDataAsync();
    }

    // Lädt die Daten, die das Formular braucht. Ablauf:
    // 1) Aktuellen User holen (für PaidBy und Admin-Prüfung).
    // 2) Kategorien und Gruppenmitglieder PARALLEL abrufen (Task.WhenAll spart Zeit).
    // 3) Kategorie-Picker befüllen.
    // 4) Für jedes Mitglied eine Split-Zeile anlegen (anfangs alle ausgewählt); dabei feststellen,
    //    ob der eingeloggte User Admin ist (steuert die Custom-Split-Berechtigung).
    // 5) NUR im Bearbeiten-Modus (ExpenseId > 0): die bestehende Ausgabe nachladen und alle Felder
    //    (Titel, Beschreibung, Betrag, Datum, Kategorie, Ort) vorbelegen sowie die gespeicherte
    //    Aufteilung wiederherstellen (passende Mitglieder anhaken und deren Beträge eintragen).
    private async Task LoadInitialDataAsync()
    {
        if (GroupId <= 0) return;
        IsBusy = true;
        HasError = false;

        try
        {
            // (1) Eingeloggten User ermitteln.
            var user = await _authService.GetCurrentUserAsync();
            _currentUserId = user.Id;

            // (2) Kategorien + Mitglieder gleichzeitig anfragen und auf beide warten.
            var categoriesTask = _tripService.GetExpenseCategoriesAsync(GroupId);
            var membersTask = _tripService.GetGroupMembersAsync(GroupId);
            await Task.WhenAll(categoriesTask, membersTask);

            // (3) Kategorie-Picker befüllen.
            Categories.Clear();
            var categories = await categoriesTask;
            foreach (var cat in categories)
                Categories.Add(cat);

            // (4) Pro Mitglied eine Split-Zeile; merken, ob der aktuelle User Admin ist.
            var members = await membersTask;
            SplitMembers.Clear();
            foreach (var member in members)
            {
                SplitMembers.Add(new ExpenseSplitMemberInput(member.UserId, member.Username, member.Role, true));
                if (member.UserId == _currentUserId)
                    IsCurrentUserAdmin = string.Equals(member.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            }

            // (5) Bearbeiten-Modus: bestehende Ausgabe laden und das Formular damit vorbelegen.
            if (ExpenseId > 0)
            {
                var expense = await _tripService.GetGroupExpenseByIdAsync(GroupId, ExpenseId);
                TitleText = expense.Title;
                Description = expense.Description ?? string.Empty;
                // Betrag mit InvariantCulture formatieren, damit immer ein Punkt als Dezimaltrenner steht.
                AmountText = expense.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                ExpenseDate = expense.ExpenseDate;
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == expense.CategoryId);

                // Ort wiederherstellen, falls die Ausgabe Koordinaten hat.
                if (expense.HasLocation)
                {
                    SelectedLatitude = (double?)expense.Latitude;
                    SelectedLongitude = (double?)expense.Longitude;
                    LocationName = $"{expense.Latitude:F4}, {expense.Longitude:F4}";
                    HasLocation = true;
                }

                // Aufteilung wiederherstellen: erst alle abwählen, dann nur die Mitglieder anhaken,
                // die in der gespeicherten Ausgabe vorkommen, und deren Betrag in die Zeile schreiben.
                foreach (var m in SplitMembers) m.IsSelected = false;
                foreach (var split in expense.Splits)
                {
                    var m = SplitMembers.FirstOrDefault(member => member.UserId == split.UserId);
                    if (m != null)
                    {
                        m.IsSelected = true;
                        m.ValueText = split.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
            }

            OnPropertyChanged(nameof(IsNotCurrentUserAdmin));
            OnPropertyChanged(nameof(CanConfigureCustomSplit));
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

    // Auto-generierter "Partial Hook": Wird automatisch aufgerufen, wenn sich SelectedSplitMode ändert
    // (CommunityToolkit erzeugt diese OnXxxChanged-Methode passend zur [ObservableProperty]).
    // Blendet die Wert-Eingabefelder nur bei "Percentage"/"Amount" ein, setzt den passenden Platzhalter
    // und aktualisiert die Live-Vorschau.
    partial void OnSelectedSplitModeChanged(string value)
    {
        ShowSplitValueInputs = value is "Percentage" or "Amount";
        SplitValueHint = value == "Percentage" ? "%" : value == "Amount" ? "0.00" : string.Empty;
        UpdateSplitPreview();
    }

    // Partial Hook für AmountText: bei jeder Änderung des Gesamtbetrags wird die Vorschau neu berechnet.
    partial void OnAmountTextChanged(string value) => UpdateSplitPreview();

    // Berechnet live, ob die manuelle Aufteilung gültig ist, und färbt die Vorschau grün (ok) oder rot (Fehler).
    // - Nur relevant bei "Percentage" oder "Amount"; bei "Equal" wird die Vorschau ausgeblendet.
    // - Es zählen nur ausgewählte Mitglieder; ist keines gewählt, gibt es nichts anzuzeigen.
    // - Percentage: Summe aller Prozentwerte muss 100 ergeben (Toleranz 0,01 wegen Rundung).
    // - Amount:     Summe aller Beträge muss exakt dem Gesamtbetrag (AmountText) entsprechen.
    private void UpdateSplitPreview()
    {
        // Bei gleichmäßiger Aufteilung ist keine Eingabe-Prüfung nötig.
        if (SelectedSplitMode is not ("Percentage" or "Amount"))
        {
            ShowSplitPreview = false;
            return;
        }

        // Nur angehakte Mitglieder beteiligen sich an der Aufteilung.
        var selected = SplitMembers.Where(m => m.IsSelected).ToList();
        if (selected.Count == 0)
        {
            ShowSplitPreview = false;
            return;
        }

        if (SelectedSplitMode == "Percentage")
        {
            // Prozentwerte aufsummieren (ungültige/leere Eingaben zählen als 0).
            var total = selected.Sum(m => decimal.TryParse(m.ValueText, out var v) ? v : 0);
            ShowSplitPreview = true;
            // Muss 100% ergeben (kleine Rundungstoleranz erlaubt).
            if (Math.Abs(total - 100) <= 0.01m)
            {
                SplitPreviewMessage = "✓ Percentages sum to 100%";
                SplitPreviewColor = Color.FromArgb("#4CAF50");
            }
            else
            {
                SplitPreviewMessage = $"Percentages sum to {total:F1}% (must equal 100%)";
                SplitPreviewColor = Color.FromArgb("#E53935");
            }
        }
        else
        {
            // "Amount"-Modus: Gesamtbetrag parsen; ohne gültigen Betrag gibt es nichts zu prüfen.
            if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var totalAmount) || totalAmount == 0)
            {
                ShowSplitPreview = false;
                return;
            }

            // Einzelbeträge aufsummieren und mit dem Gesamtbetrag vergleichen.
            var splitSum = selected.Sum(m => decimal.TryParse(m.ValueText, out var v) ? v : 0);
            ShowSplitPreview = true;
            if (Math.Abs(splitSum - totalAmount) <= 0.01m)
            {
                SplitPreviewMessage = $"✓ Amounts sum to {totalAmount:F2}";
                SplitPreviewColor = Color.FromArgb("#4CAF50");
            }
            else
            {
                SplitPreviewMessage = $"Amounts sum to {splitSum:F2} (must equal {totalAmount:F2})";
                SplitPreviewColor = Color.FromArgb("#E53935");
            }
        }
    }

    // Speichert die übergebenen Koordinaten im ViewModel und zeigt sie als "lat, lng" an.
    // Wird aus dem Code-Behind der View aufgerufen, wenn der User auf die Karte tippt (Map-Tap).
    public void SetLocation(double latitude, double longitude)
    {
        SelectedLatitude = latitude;
        SelectedLongitude = longitude;
        LocationName = $"{latitude:F4}, {longitude:F4}";
        HasLocation = true;
        HasLocationError = false;
    }

    // RelayCommand ("Aktuellen Standort verwenden"): fragt zuerst die GPS-Berechtigung an
    // (LocationWhenInUse). Wird sie verweigert, Fehlertext und Abbruch. Sonst die aktuelle Position
    // über Geolocation lesen, per SetLocation speichern und anschließend einen Reverse-Geocode machen
    // (Koordinaten -> lesbare Adresse: Ort, Stadt, Land), der dann als LocationName angezeigt wird.
    [RelayCommand]
    private async Task UseCurrentLocationAsync()
    {
        IsLoadingLocation = true;
        HasLocationError = false;
        OnPropertyChanged(nameof(IsNotLoadingLocation));

        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                LocationError = "Location permission denied.";
                HasLocationError = true;
                return;
            }

            var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium));

            if (location == null)
            {
                LocationError = "Could not determine location.";
                HasLocationError = true;
                return;
            }

            SetLocation(location.Latitude, location.Longitude);

            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
            var place = placemarks?.FirstOrDefault();
            if (place != null)
            {
                var parts = new[] { place.FeatureName, place.Locality, place.CountryName }
                    .Where(s => !string.IsNullOrWhiteSpace(s));
                LocationName = string.Join(", ", parts);
            }
        }
        catch (Exception ex)
        {
            LocationError = ex.Message;
            HasLocationError = true;
        }
        finally
        {
            IsLoadingLocation = false;
            OnPropertyChanged(nameof(IsNotLoadingLocation));
        }
    }

    // RelayCommand ("Adresse suchen"): wandelt die getippte Adresse (LocationAddress) per Geocoding in
    // Koordinaten um (das Gegenstück zum Reverse-Geocode oben). Findet sich nichts, Fehlertext.
    // Bei Erfolg werden die Koordinaten via SetLocation gespeichert und die eingegebene Adresse als Name übernommen.
    [RelayCommand]
    private async Task GeocodeAddressAsync()
    {
        if (string.IsNullOrWhiteSpace(LocationAddress)) return;

        IsLoadingLocation = true;
        HasLocationError = false;
        OnPropertyChanged(nameof(IsNotLoadingLocation));

        try
        {
            var locations = await Geocoding.Default.GetLocationsAsync(LocationAddress);
            var loc = locations?.FirstOrDefault();
            if (loc == null)
            {
                LocationError = "Address not found.";
                HasLocationError = true;
                return;
            }

            SetLocation(loc.Latitude, loc.Longitude);
            LocationName = LocationAddress;
        }
        catch (Exception ex)
        {
            LocationError = ex.Message;
            HasLocationError = true;
        }
        finally
        {
            IsLoadingLocation = false;
            OnPropertyChanged(nameof(IsNotLoadingLocation));
        }
    }

    // RelayCommand: Entfernt den gewählten Ort wieder, indem alle Standort-Felder zurückgesetzt werden.
    [RelayCommand]
    private void ClearLocation()
    {
        SelectedLatitude = null;
        SelectedLongitude = null;
        LocationName = string.Empty;
        LocationAddress = string.Empty;
        HasLocation = false;
    }

    // RelayCommand für den Speichern-Button. Führt zuerst die komplette Validierung durch und sendet
    // die Ausgabe dann an die API. Validierungsschritte (jeweils: Fehlertext setzen + abbrechen):
    //   a) Titel darf nicht leer sein.
    //   b) Betrag muss eine gültige Zahl > 0 sein (InvariantCulture, damit "." als Dezimaltrenner gilt).
    //   c) Eine Kategorie muss gewählt sein.
    //   d) Mindestens ein Mitglied muss für die Aufteilung ausgewählt sein.
    //   e) Bei "Percentage": Summe der Prozente muss 100 ergeben.
    //   f) Bei "Amount":     Summe der Einzelbeträge muss dem Gesamtbetrag entsprechen.
    // Danach wird ein CreateExpenseRequest-DTO gebaut und – je nach Modus – entweder über
    // UpdateExpenseAsync (Bearbeiten) oder CreateExpenseAsync (Neu) an die API geschickt.
    // Bei Erfolg eine Ebene zurück navigieren.
    [RelayCommand]
    private async Task SaveExpenseAsync()
    {
        HasError = false;

        // (a) Titel-Pflichtfeld.
        if (string.IsNullOrWhiteSpace(TitleText))
        {
            ErrorMessage = "Please enter a title.";
            HasError = true;
            return;
        }

        // (b) Betrag parsen und auf > 0 prüfen.
        if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ErrorMessage = "Please enter a valid amount greater than 0.";
            HasError = true;
            return;
        }

        // (c) Kategorie muss gewählt sein.
        if (SelectedCategory == null)
        {
            ErrorMessage = "Please select a category.";
            HasError = true;
            return;
        }

        // (d) Aufteilung braucht mindestens ein beteiligtes Mitglied.
        var selectedMembers = SplitMembers.Where(m => m.IsSelected).ToList();
        if (selectedMembers.Count == 0)
        {
            ErrorMessage = "Select at least one member to split with.";
            HasError = true;
            return;
        }

        // (e) Prozent-Aufteilung muss exakt 100% ergeben.
        if (SelectedSplitMode == "Percentage")
        {
            var pctTotal = selectedMembers.Sum(m => decimal.TryParse(m.ValueText, out var v) ? v : 0);
            if (Math.Abs(pctTotal - 100) > 0.01m)
            {
                ErrorMessage = $"Percentages must sum to 100% (currently {pctTotal:F1}%).";
                HasError = true;
                return;
            }
        }
        // (f) Betrags-Aufteilung muss exakt dem Gesamtbetrag entsprechen.
        else if (SelectedSplitMode == "Amount")
        {
            var splitSum = selectedMembers.Sum(m => decimal.TryParse(m.ValueText, out var v) ? v : 0);
            if (Math.Abs(splitSum - amount) > 0.01m)
            {
                ErrorMessage = $"Split amounts must sum to {amount:F2}.";
                HasError = true;
                return;
            }
        }

        IsBusy = true;

        try
        {
            // DTO für die API zusammenbauen. Wichtig bei der Aufteilung:
            // - "Equal": nur die Liste der UserIds senden, der Server teilt gleichmäßig auf (SplitUserIds).
            // - "Percentage"/"Amount": pro Mitglied einen Wert senden (SplitAllocations); jeweils das
            //   nicht benötigte Feld bleibt null. PaidByUserId = aktueller User, sofern bekannt.
            //   Koordinaten werden nur mitgegeben, wenn tatsächlich ein Ort gesetzt ist.
            var request = new CreateExpenseRequest
            {
                Title = TitleText.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                Amount = amount,
                CategoryId = SelectedCategory.Id,
                PaidByUserId = _currentUserId > 0 ? _currentUserId : null,
                ExpenseDate = ExpenseDate,
                Latitude = HasLocation && SelectedLatitude.HasValue ? (decimal?)SelectedLatitude.Value : null,
                Longitude = HasLocation && SelectedLongitude.HasValue ? (decimal?)SelectedLongitude.Value : null,
                SplitMode = SelectedSplitMode,
                SplitUserIds = SelectedSplitMode == "Equal"
                    ? selectedMembers.Select(m => m.UserId).ToList()
                    : null,
                SplitAllocations = SelectedSplitMode != "Equal"
                    ? selectedMembers
                        .Select(m => new ExpenseSplitInputRequest
                        {
                            UserId = m.UserId,
                            Value = decimal.TryParse(m.ValueText, out var v) ? v : 0
                        })
                        .ToList()
                    : null
            };

            // Bearbeiten-Modus (ExpenseId > 0) -> PUT/Update, sonst -> POST/Create.
            if (ExpenseId > 0)
            {
                await _tripService.UpdateExpenseAsync(GroupId, ExpenseId, request);
            }
            else
            {
                await _tripService.CreateExpenseAsync(GroupId, request);
            }

            // Erfolg: zurück zur vorherigen Seite (Gruppen-Detail bzw. Ausgaben-Detail).
            await Shell.Current.GoToAsync("..");
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

    // RelayCommand für Abbrechen: navigiert eine Ebene zurück (".."), ohne die Ausgabe zu speichern.
    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
