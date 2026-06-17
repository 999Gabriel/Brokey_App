namespace Brokey_APP.Models;

// Datencontainer für den Registrierungs-Body (JSON), gesendet von der Register-Seite an POST /api/auth/register.
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = "EUR";   // Standard-Währung des Nutzers; vorbelegt mit "EUR".
}

