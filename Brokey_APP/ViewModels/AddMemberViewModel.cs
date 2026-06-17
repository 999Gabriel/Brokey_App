using Brokey_APP.Models;
using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel der "Mitglied hinzufügen"-Seite. Erreichbar von der Gruppen-Detailseite (per groupId/groupName).
// Nimmt einen Username oder eine Email entgegen und fügt diese Person der Gruppe hinzu.
// Implementiert IQueryAttributable, um groupId und groupName aus der Navigation zu empfangen.
public partial class AddMemberViewModel : BaseViewModel, IQueryAttributable
{
    // Per DI eingespeister Trip-Service für die HTTP-Aufrufe.
    private readonly ITripService _tripService;

    // Id und Name der Zielgruppe, kommen als Query-Parameter über die Navigation rein.
    [ObservableProperty]
    private int _groupId;

    [ObservableProperty]
    private string _groupName = string.Empty;

    // Eingabe-Feld: Username ODER Email der hinzuzufügenden Person.
    [ObservableProperty]
    private string _usernameOrEmail = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public AddMemberViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Add Member";
    }

    // IQueryAttributable: Liest beim Navigieren groupId und groupName aus der Route
    // (von GroupDetailViewModel.OpenAddMemberAsync gesetzt); der Name wird wieder dekodiert.
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
        }
    }

    // RelayCommand für den "Hinzufügen"-Button. Prüft, dass eine gültige Gruppe vorliegt und das
    // Eingabefeld nicht leer ist – sonst Fehlertext und Abbruch. Sonst baut es ein
    // AddGroupMemberRequest (Rolle fest "Member"), ruft TripService.AddGroupMemberAsync (POST an die
    // API) auf und navigiert bei Erfolg eine Ebene zurück zur Gruppen-Detailseite.
    [RelayCommand]
    private async Task AddMemberAsync()
    {
        if (GroupId <= 0 || string.IsNullOrWhiteSpace(UsernameOrEmail))
        {
            ErrorMessage = "Please enter a username or email.";
            HasError = true;
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            await _tripService.AddGroupMemberAsync(GroupId, new AddGroupMemberRequest
            {
                UsernameOrEmail = UsernameOrEmail.Trim(),
                Role = "Member"
            });

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

    // RelayCommand für Abbrechen: navigiert eine Ebene zurück (".."), ohne ein Mitglied hinzuzufügen.
    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
