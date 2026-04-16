namespace Brokey_APP.Services;

public class TokenStorageService : ITokenStorageService
{
    private const string TokenKey = "auth_token";
    private string? _cachedToken;

    public TokenStorageService()
    {
        _cachedToken = Preferences.Default.Get(TokenKey, default(string));
    }

    public async Task SaveTokenAsync(string token)
    {
        _cachedToken = token;
        Preferences.Default.Set(TokenKey, token);

        try
        {
            await SecureStorage.Default.SetAsync(TokenKey, token);
        }
        catch
        {
            try
            {
                SecureStorage.Default.Remove(TokenKey);
            }
            catch
            {
                // Ignore cleanup failures on unsupported platforms.
            }
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cachedToken))
        {
            return _cachedToken;
        }

        var preferenceToken = Preferences.Default.Get(TokenKey, default(string));
        if (!string.IsNullOrWhiteSpace(preferenceToken))
        {
            _cachedToken = preferenceToken;
            return preferenceToken;
        }

        try
        {
            var secureToken = await SecureStorage.Default.GetAsync(TokenKey);
            if (!string.IsNullOrWhiteSpace(secureToken))
            {
                _cachedToken = secureToken;
                Preferences.Default.Set(TokenKey, secureToken);
                return secureToken;
            }
        }
        catch
        {
            // Fall back to Preferences below.
        }

        return preferenceToken;
    }

    public Task ClearTokenAsync()
    {
        _cachedToken = null;

        try
        {
            SecureStorage.Default.Remove(TokenKey);
        }
        catch
        {
            // Ignore unsupported secure storage cleanup failures.
        }

        Preferences.Default.Remove(TokenKey);
        return Task.CompletedTask;
    }

    public bool HasStoredToken()
    {
        return !string.IsNullOrWhiteSpace(_cachedToken) ||
               !string.IsNullOrWhiteSpace(Preferences.Default.Get(TokenKey, default(string)));
    }
}
