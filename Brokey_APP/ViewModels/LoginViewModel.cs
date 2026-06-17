using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel des Login-Screens (erster Screen in der AuthShell für nicht eingeloggte User).
// Nimmt Email + Passwort entgegen, meldet den User über den AuthService an und schaltet bei
// Erfolg auf die AppShell (Haupt-App) um. Von hier aus geht es auch zur Registrierung.
public partial class LoginViewModel : BaseViewModel
{
    // Per Dependency Injection eingespeister Service, der die HTTP-Anfragen an den API-Server kapselt.
    private readonly IAuthService _authService;

    // Eingabe-Felder, gebunden an die TextBoxen im XAML (Zwei-Wege-Bindung).
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    // Fehlertext, der bei einem Login-Fehler angezeigt wird.
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Steuert, ob die rote Fehlermeldung in der UI sichtbar ist (IsVisible-Bindung).
    [ObservableProperty]
    private bool _hasError;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Login";
    }

    // RelayCommand für den Login-Button.
    // 1) Prüft, dass Email UND Passwort ausgefüllt sind – sonst Fehlertext und Abbruch.
    // 2) Setzt IsBusy=true (Ladeanzeige) und ruft AuthService.LoginAsync auf.
    //    Der Service schickt einen POST an /api/auth/login und speichert bei Erfolg den JWT-Token.
    // 3) Bei Erfolg: tauscht die Wurzelseite gegen die AppShell aus -> User ist in der Haupt-App.
    // 4) Bei Fehler (z.B. falsches Passwort, Server nicht erreichbar): zeigt die Exception-Nachricht an.
    // 5) finally: IsBusy wird in jedem Fall wieder auf false gesetzt.
    [RelayCommand]
    private async Task LoginAsync()
    {
        // Eingabe-Validierung: leere Felder abfangen, bevor ein HTTP-Request rausgeht.
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter your email and password.";
            HasError = true;
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            // Email getrimmt (ohne Leerzeichen) an den Service; Passwort unverändert.
            await _authService.LoginAsync(Email.Trim(), Password);

            // Wurzelseite des Fensters auf die AppShell setzen = Wechsel in die eingeloggte Haupt-App.
            Application.Current!.Windows[0].Page = new AppShell();
        }
        catch (Exception ex)
        {
            // Fehlermeldung aus dem Service/Server an die UI durchreichen.
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // RelayCommand: Wechselt per Shell zur Registrierungs-Seite.
    // "//register" ist eine absolute Route innerhalb der AuthShell.
    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        await Shell.Current.GoToAsync("//register");
    }

    // RelayCommand: Zeigt nur einen Hinweis-Dialog an – die Passwort-Zurücksetzen-Funktion
    // ist noch nicht implementiert. Keine Navigation, kein Service-Aufruf.
    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
            "Forgot Password",
            "Password reset is not yet available. Please contact support.",
            "OK");
    }
}

