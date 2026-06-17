namespace API_Server.DTOs;

// Antwort der User-/Profil-Endpoints – öffentliche Userdaten (ohne Passwort) für ProfileViewModel.
public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
}

