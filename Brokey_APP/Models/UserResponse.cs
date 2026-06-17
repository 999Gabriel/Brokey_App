namespace Brokey_APP.Models;

// Datencontainer für das Nutzerprofil (Antwort von GET /api/auth/me). Verwendet im Profil-Tab und auf der Startseite.
public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }   // Optionaler Pfad/URL zum Profilbild (null, wenn keins gesetzt).
    public DateTime CreatedAt { get; set; }          // Zeitpunkt der Konto-Erstellung (z. B. "Mitglied seit").
}

