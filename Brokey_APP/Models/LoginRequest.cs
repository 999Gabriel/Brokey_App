namespace Brokey_APP.Models;

// Datencontainer für den Login-Request-Body (JSON), gesendet von der Login-Seite an POST /api/auth/login.
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

