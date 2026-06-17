using Brokey_APP.Services;

namespace Brokey_APP;

// Einstiegspunkt der MAUI-App. Entscheidet beim Start, welche Shell (Login oder eingeloggte App) angezeigt wird.
public partial class App : Application
{
    // Dienst zum Lesen/Schreiben des gespeicherten JWT-Tokens (Memory → Preferences → SecureStorage).
    private readonly ITokenStorageService _tokenStorageService;

    // Konstruktor: bekommt den TokenStorageService per Dependency Injection und merkt ihn sich.
    public App(ITokenStorageService tokenStorageService)
    {
        // TokenStorageService für die spätere Start-Entscheidung speichern.
        _tokenStorageService = tokenStorageService;
        // Lädt die in App.xaml definierten Ressourcen (z. B. Farben, Styles).
        InitializeComponent();
    }

    // Wird von MAUI beim App-Start aufgerufen, um das erste Fenster (und damit die Start-Shell) zu erzeugen.
    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Prüft, ob bereits ein gültiges Token gespeichert ist (= Nutzer war eingeloggt).
        var isAuthenticated = _tokenStorageService.HasStoredToken();
        // Entscheidet beim App-Start: AppShell (Token vorhanden) oder AuthShell (nicht eingeloggt).
        return new Window(isAuthenticated ? new AppShell() : new AuthShell());
    }
}
