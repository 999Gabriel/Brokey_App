namespace Brokey_APP.Services;

/// <summary>
/// Central place for API configuration.
/// </summary>
public static class ApiConfig
{
    // ── Development URLs ──
    // Use localhost for Mac/Windows desktop, 10.0.2.2 for Android emulator
#if ANDROID
    public const string BaseUrl = "http://10.0.2.2:5224";
#else
    public const string BaseUrl = "http://localhost:5224";
#endif

    public static Uri BaseUri => new(BaseUrl);
}

