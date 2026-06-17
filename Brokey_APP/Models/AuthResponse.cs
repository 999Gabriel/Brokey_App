namespace Brokey_APP.Models;

// Datencontainer für die Antwort von Login/Registrierung. Enthält die Nutzer-Eckdaten und vor allem den JWT.
public class AuthResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;   // Der JWT, der danach im TokenStorageService gespeichert wird.
}
