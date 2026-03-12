namespace Brokey_APP.Services;

public class TokenStorageService : ITokenStorageService
{
    private const string TokenKey = "auth_token";

    public Task SaveTokenAsync(string token)
    {
        return SecureStorage.Default.SetAsync(TokenKey, token);
    }

    public Task<string?> GetTokenAsync()
    {
        return SecureStorage.Default.GetAsync(TokenKey);
    }

    public Task ClearTokenAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        return Task.CompletedTask;
    }

    public bool HasStoredToken()
    {
        return !string.IsNullOrWhiteSpace(GetTokenAsync().GetAwaiter().GetResult());
    }
}
