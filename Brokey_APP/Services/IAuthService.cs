using Brokey_APP.Models;

namespace Brokey_APP.Services;

// Vertrag (Interface) für alle Authentifizierungs-Funktionen. Definiert NUR, WAS möglich ist, nicht WIE.
// Implementiert von AuthService; genutzt von LoginViewModel, RegisterViewModel, HomeViewModel, ProfileViewModel.
// Über dieses Interface kann man die echte Implementierung z. B. in Tests durch eine Attrappe ersetzen.
public interface IAuthService
{
    // Loggt den Nutzer mit E-Mail + Passwort ein und liefert die Auth-Daten inkl. JWT.
    Task<AuthResponse> LoginAsync(string email, string password);
    // Registriert einen neuen Nutzer (mit Wunsch-Basiswährung) und loggt ihn direkt ein.
    Task<AuthResponse> RegisterAsync(string username, string email, string password, string baseCurrency);
    // Lädt das Profil des aktuell eingeloggten Nutzers.
    Task<UserResponse> GetCurrentUserAsync();
    // Loggt den Nutzer aus (löscht lokal den Token).
    Task LogoutAsync();
    // Gibt true zurück, wenn ein Token gespeichert ist (= Nutzer gilt als eingeloggt).
    Task<bool> IsAuthenticatedAsync();
}

