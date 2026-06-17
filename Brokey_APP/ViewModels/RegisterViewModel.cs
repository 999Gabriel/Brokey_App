using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel des Registrierungs-Screens (in der AuthShell). Erreichbar vom Login-Screen aus.
// Legt über den AuthService einen neuen Benutzer an und wechselt bei Erfolg in die Haupt-App (AppShell).
public partial class RegisterViewModel : BaseViewModel
{
    // Per Dependency Injection eingespeister Service für die Auth-HTTP-Aufrufe an den API-Server.
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    // Gewählte Heimat-/Standardwährung des Users (Vorauswahl EUR), gebunden an einen Picker.
    [ObservableProperty]
    private string _baseCurrency = "EUR";

    // Fehlertext + Sichtbarkeits-Flag für die rote Fehlermeldung in der UI.
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    // Auswahlliste der unterstützten Währungen; befüllt den Währungs-Picker im XAML.
    public List<string> Currencies { get; } = new()
    {
        "EUR", "USD", "GBP", "CHF", "JPY", "AUD", "CAD",
        "TRY", "THB", "MXN", "BRL", "CZK", "HUF", "PLN",
        "SEK", "NOK", "DKK"
    };

    public RegisterViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Register";
    }

    // RelayCommand für den "Registrieren"-Button. Validierung in drei Schritten:
    // 1) Alle Felder müssen ausgefüllt sein.
    // 2) Passwort und Wiederholung müssen identisch sein.
    // 3) Passwort muss mindestens 6 Zeichen lang sein.
    // Schlägt eine Prüfung fehl: Fehlertext setzen und abbrechen (kein HTTP-Request).
    // Sind alle Prüfungen ok: AuthService.RegisterAsync schickt einen POST an /api/auth/register,
    // bei Erfolg wird der Token gespeichert und auf die AppShell (Haupt-App) umgeschaltet.
    [RelayCommand]
    private async Task RegisterAsync()
    {
        // Schritt 1: Pflichtfelder prüfen.
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "Please fill in all fields.";
            HasError = true;
            return;
        }

        // Schritt 2: Beide Passwort-Eingaben müssen übereinstimmen.
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            HasError = true;
            return;
        }

        // Schritt 3: Mindestlänge des Passworts.
        if (Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters.";
            HasError = true;
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            // Registrierungs-Daten an den Service; Username/Email getrimmt.
            await _authService.RegisterAsync(
                Username.Trim(),
                Email.Trim(),
                Password,
                BaseCurrency);

            // Erfolg: Wurzelseite auf AppShell setzen -> direkt eingeloggt in der Haupt-App.
            Application.Current!.Windows[0].Page = new AppShell();
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

    // RelayCommand: Wechselt zurück zur Login-Seite ("//login" = absolute Route in der AuthShell).
    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }
}

