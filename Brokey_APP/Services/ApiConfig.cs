namespace Brokey_APP.Services;

/// <summary>
/// Central place for API configuration.
/// </summary>
public static class ApiConfig
{
    // Use HTTPS directly so the client never loses auth headers on HTTP->HTTPS redirects.
    // Android emulator needs 10.0.2.2 to reach the host machine.
#if ANDROID
    public const string BaseUrl = "https://10.0.2.2:7221";
#else
    public const string BaseUrl = "https://localhost:7221";
#endif

    public static Uri BaseUri => new(BaseUrl);
}
