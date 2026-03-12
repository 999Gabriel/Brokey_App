using Brokey_APP.Models;

namespace Brokey_APP.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(string email, string password);
    Task<AuthResponse> RegisterAsync(string username, string email, string password, string baseCurrency);
    Task<UserResponse> GetCurrentUserAsync();
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
}

