using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel der Ausgaben-Detailseite. Erreichbar von der Gruppen-Detailseite und vom Home-Screen
// (per groupId/expenseId). Zeigt alle Details einer einzelnen Ausgabe und bietet Bearbeiten/Löschen an.
// Implementiert IQueryAttributable, um groupId und expenseId aus der Navigation zu empfangen.
public partial class ExpenseDetailViewModel : BaseViewModel, IQueryAttributable
{
    // Per DI eingespeister Trip-Service für die HTTP-Aufrufe.
    private readonly ITripService _tripService;

    // groupId + expenseId identifizieren die anzuzeigende Ausgabe; kommen als Query-Parameter rein.
    [ObservableProperty] private int _groupId;
    [ObservableProperty] private int _expenseId;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;
    // Die geladene Ausgabe selbst; an die Detail-Controls im XAML gebunden.
    [ObservableProperty] private ExpenseResponse? _expense;
    // Hilfs-Flags zur Sichtbarkeitssteuerung: ob ein Ort, eine Beschreibung bzw. eine Aufteilung
    // (Splits) vorhanden ist. So werden in der UI nur die Abschnitte gezeigt, die Daten haben.
    [ObservableProperty] private bool _hasLocation;
    [ObservableProperty] private bool _hasDescription;
    [ObservableProperty] private bool _hasSplits;

    // Umkehrung von IsBusy: erlaubt es, Inhalte erst nach dem Laden einzublenden (IsVisible-Bindung).
    public bool IsNotBusy => !IsBusy;

    public ExpenseDetailViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Expense";
    }

    // IQueryAttributable: Liest beim Navigieren groupId und expenseId aus der Route (z.B. von
    // GroupDetailViewModel oder HomeViewModel gesetzt) und startet danach den Ladevorgang.
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
        }

        _ = LoadExpenseAsync();
    }

    // RelayCommand: Lädt die einzelne Ausgabe über TripService.GetGroupExpenseByIdAsync
    // (GET /api/groups/{groupId}/expenses/{expenseId}) in die Expense-Property und leitet daraus die
    // Hilfs-Flags HasLocation/HasDescription/HasSplits ab. IsNotBusy wird mehrfach benachrichtigt,
    // damit die UI den Inhalt korrekt ein-/ausblendet.
    [RelayCommand]
    private async Task LoadExpenseAsync()
    {
        if (GroupId <= 0 || ExpenseId <= 0) return;
        IsBusy = true;
        OnPropertyChanged(nameof(IsNotBusy));
        HasError = false;

        try
        {
            Expense = await _tripService.GetGroupExpenseByIdAsync(GroupId, ExpenseId);
            Title = Expense.Title;
            HasLocation = Expense.HasLocation;
            HasDescription = !string.IsNullOrWhiteSpace(Expense.Description);
            HasSplits = Expense.Splits.Count > 0;
            OnPropertyChanged(nameof(IsNotBusy));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    // RelayCommand: Öffnet die "Ausgabe hinzufügen"-Seite im Bearbeiten-Modus (Route "add-expense").
    // Durch das Mitsenden von expenseId erkennt diese Seite, dass eine bestehende Ausgabe geändert
    // werden soll (statt eine neue anzulegen). groupName wird URL-enkodiert übergeben.
    [RelayCommand]
    private async Task EditAsync()
    {
        if (Expense == null) return;
        await Shell.Current.GoToAsync($"add-expense?groupId={GroupId}&expenseId={ExpenseId}&groupName={Uri.EscapeDataString(Expense.GroupName)}");
    }

    // RelayCommand: Löscht die Ausgabe. Zeigt zuerst einen Bestätigungs-Dialog; bei "Cancel" Abbruch.
    // Bei Bestätigung: TripService.DeleteExpenseAsync (DELETE an die API) und danach eine Ebene zurück.
    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Expense == null) return;

        var confirmed = await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
            "Delete Expense",
            $"Delete \"{Expense.Title}\"? This cannot be undone.",
            "Delete",
            "Cancel");

        if (!confirmed) return;

        IsBusy = true;
        HasError = false;

        try
        {
            await _tripService.DeleteExpenseAsync(GroupId, ExpenseId);
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
}
