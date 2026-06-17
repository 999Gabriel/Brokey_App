using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel des Profil-Screens (Profile-Tab der AppShell). Zeigt die Daten des eingeloggten Users
// (Name, Email, Standardwährung) und ermöglicht das Ausloggen.
public partial class ProfileViewModel : BaseViewModel
{
    // Per DI eingespeister Auth-Service für User-Daten und Logout.
    private readonly IAuthService _authService;

    // An die UI gebundene Profilfelder. Anfangs mit Platzhaltern belegt, bis die echten Daten geladen sind.
    [ObservableProperty]
    private string _username = "Traveler";

    [ObservableProperty]
    private string _email = "traveler@brokey.app";

    [ObservableProperty]
    private string _baseCurrency = "EUR";

    public ProfileViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Profile";

        // Lädt die Profildaten direkt beim Erstellen im Hintergrund.
        _ = LoadUserInfoAsync();
    }

    // Holt den eingeloggten User über AuthService.GetCurrentUserAsync (GET /api/auth/me) und füllt
    // Username, Email und BaseCurrency. Fehler werden bewusst ignoriert, damit die Platzhalter
    // stehen bleiben, wenn man offline ist (offline-safe).
    private async Task LoadUserInfoAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            Username = user.Username;
            Email = user.Email;
            BaseCurrency = user.BaseCurrency;
        }
        catch
        {
            // Ignore – keep defaults
        }
    }

    // RelayCommand für Pull-to-Refresh: lädt die Profildaten erneut über den AuthService.
    // Steuert IsBusy für die Lade-/Refresh-Anzeige; Fehler werden ignoriert (gecachte Daten bleiben).
    [RelayCommand]
    private async Task RefreshProfileAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var user = await _authService.GetCurrentUserAsync();
            Username = user.Username;
            Email = user.Email;
            BaseCurrency = user.BaseCurrency;
        }
        catch
        {
            // Offline or error – keep cached data
        }
        finally
        {
            IsBusy = false;
        }
    }

    // RelayCommand für den Logout-Button: ruft AuthService.LogoutAsync (löscht den gespeicherten
    // JWT-Token aus dem TokenStorage) und tauscht die Wurzelseite gegen die AuthShell (Login) aus.
    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        Application.Current!.Windows[0].Page = new AuthShell();
    }
}
