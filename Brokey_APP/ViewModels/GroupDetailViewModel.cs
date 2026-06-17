using System.Collections.ObjectModel;
using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel der Gruppen-Detailseite. Erreichbar von der Trip-Detailseite (per groupId/groupName).
// Zeigt die Mitglieder, die Ausgaben und die Settlement-Berechnung einer Gruppe (wer schuldet wem
// wie viel). Bietet Aktionen zum Hinzufügen von Mitgliedern/Ausgaben und zum Entfernen von Mitgliedern.
// Implementiert IQueryAttributable, um groupId und groupName aus der Navigation zu empfangen.
public partial class GroupDetailViewModel : BaseViewModel, IQueryAttributable
{
    // Per DI eingespeister Trip-Service für die HTTP-Aufrufe.
    private readonly ITripService _tripService;

    // Die vier an die UI gebundenen Listen der Gruppe:
    // Members          = Mitglieder der Gruppe
    // Expenses         = erfasste Ausgaben
    // SettlementTransfers = konkrete Ausgleichszahlungen ("X zahlt Y den Betrag Z")
    // SettlementBalances  = Saldo pro Mitglied (positiv = bekommt Geld, negativ = schuldet Geld)
    public ObservableCollection<GroupMemberResponse> Members { get; } = [];
    public ObservableCollection<ExpenseResponse> Expenses { get; } = [];
    public ObservableCollection<SettlementTransferResponse> SettlementTransfers { get; } = [];
    public ObservableCollection<SettlementBalanceResponse> SettlementBalances { get; } = [];

    // Id und Name der aktuell angezeigten Gruppe, kommen als Query-Parameter über die Navigation rein.
    [ObservableProperty]
    private int _groupId;

    [ObservableProperty]
    private string _groupName = string.Empty;

    // Fehlertext + Sichtbarkeits-Flag für die Fehleranzeige.
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    // Berechnete Properties zur Sichtbarkeitssteuerung der UI (Liste vs. Leer-Hinweis).
    // Da sie von Collections abhängen, werden sie nach dem Laden manuell per OnPropertyChanged
    // benachrichtigt (siehe LoadMembersAsync). ExpensesCount wird z.B. für eine Zähler-Anzeige genutzt.
    public bool HasExpenses => Expenses.Count > 0;
    public bool HasSettlementTransfers => SettlementTransfers.Count > 0;
    public bool IsExpensesEmpty => Expenses.Count == 0;
    public bool IsSettlementEmpty => SettlementTransfers.Count == 0;
    public int ExpensesCount => Expenses.Count;

    public GroupDetailViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Group";
    }

    // IQueryAttributable: Wird beim Navigieren zu dieser Seite von der Shell aufgerufen.
    // Liest groupId und groupName aus der Route (von TripDetailViewModel.OpenGroupAsync gesetzt).
    // Der Name wird wieder dekodiert (Uri.UnescapeDataString, Gegenstück zum Enkodieren beim Senden)
    // und als Seitentitel verwendet. Anschließend startet der Ladevorgang.
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("groupId", out var rawGroupId) &&
            int.TryParse(rawGroupId?.ToString(), out var groupId))
        {
            GroupId = groupId;
        }

        if (query.TryGetValue("groupName", out var rawGroupName))
        {
            GroupName = Uri.UnescapeDataString(rawGroupName?.ToString() ?? string.Empty);
            Title = GroupName;
        }

        _ = LoadMembersAsync();
    }

    // RelayCommand: Lädt nacheinander Mitglieder, Ausgaben und Settlement-Daten der Gruppe über drei
    // Service-Aufrufe (GetGroupMembersAsync, GetGroupExpensesAsync, GetGroupSettlementAsync = jeweils
    // ein GET an die API). Danach werden alle vier Collections geleert und neu befüllt; das Settlement
    // liefert sowohl Transfers (Zahlungen) als auch Balances (Salden). Zum Schluss werden die von
    // den Collections abhängigen Anzeige-Properties manuell benachrichtigt.
    [RelayCommand]
    private async Task LoadMembersAsync()
    {
        if (GroupId <= 0 || IsBusy)
        {
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            var members = await _tripService.GetGroupMembersAsync(GroupId);
            var expenses = await _tripService.GetGroupExpensesAsync(GroupId);
            var settlement = await _tripService.GetGroupSettlementAsync(GroupId);

            Members.Clear();
            Expenses.Clear();
            SettlementTransfers.Clear();
            SettlementBalances.Clear();

            foreach (var member in members)
            {
                Members.Add(member);
            }

            foreach (var expense in expenses)
            {
                Expenses.Add(expense);
            }

            foreach (var transfer in settlement.Transfers)
            {
                SettlementTransfers.Add(transfer);
            }

            foreach (var balance in settlement.Balances)
            {
                SettlementBalances.Add(balance);
            }

            OnPropertyChanged(nameof(HasExpenses));
            OnPropertyChanged(nameof(HasSettlementTransfers));
            OnPropertyChanged(nameof(IsExpensesEmpty));
            OnPropertyChanged(nameof(IsSettlementEmpty));
            OnPropertyChanged(nameof(ExpensesCount));
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

    // RelayCommand: Öffnet die "Mitglied hinzufügen"-Seite (Route "add-member"); gibt groupId und
    // den URL-enkodierten groupName mit, damit die Zielseite weiß, zu welcher Gruppe es geht.
    [RelayCommand]
    private async Task OpenAddMemberAsync()
    {
        await Shell.Current.GoToAsync(
            $"add-member?groupId={GroupId}&groupName={Uri.EscapeDataString(GroupName)}");
    }

    // RelayCommand: Öffnet die "Ausgabe hinzufügen"-Seite (Route "add-expense") mit groupId und groupName.
    [RelayCommand]
    private async Task OpenAddExpenseAsync()
    {
        await Shell.Current.GoToAsync(
            $"add-expense?groupId={GroupId}&groupName={Uri.EscapeDataString(GroupName)}");
    }

    // RelayCommand: Öffnet die Detailseite einer angetippten Ausgabe (Route "expense-detail").
    // Der Parameter "expense" stammt aus der getippten Listenzeile; groupId und expenseId werden übergeben.
    [RelayCommand]
    private async Task OpenExpenseDetailAsync(ExpenseResponse? expense)
    {
        if (expense == null) return;
        await Shell.Current.GoToAsync($"expense-detail?groupId={GroupId}&expenseId={expense.Id}");
    }

    // RelayCommand: Entfernt ein Mitglied aus der Gruppe. Zeigt zuerst einen Bestätigungs-Dialog;
    // bei "Cancel" Abbruch. Bei Bestätigung wird TripService.RemoveGroupMemberAsync (DELETE an die API)
    // mit groupId und member.UserId aufgerufen und danach die Liste neu geladen.
    [RelayCommand]
    private async Task RemoveMemberAsync(GroupMemberResponse? member)
    {
        if (member == null)
        {
            return;
        }

        bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
            "Remove Member",
            $"Remove {member.Username} from {GroupName}?",
            "Remove",
            "Cancel");

        if (!confirm)
        {
            return;
        }

        try
        {
            await _tripService.RemoveGroupMemberAsync(GroupId, member.UserId);
            await LoadMembersAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }
}
