namespace API_Server.DTOs;

// Antwort von POST /api/auth/login bzw. /register – Userdaten inkl. JWT-Token für den Client.
public class AuthResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = string.Empty;  // bevorzugte Währung des Users (z.B. EUR)
    public string Token { get; set; } = string.Empty;          // signierter JWT; Client speichert ihn und sendet ihn als "Authorization: Bearer <Token>"
}
