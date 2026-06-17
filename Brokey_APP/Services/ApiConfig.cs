namespace Brokey_APP.Services;

/// <summary>
/// Zentrale Stelle für die API-Konfiguration (Basis-URL des Servers).
/// </summary>
// static = es werden keine Instanzen erzeugt; man greift überall direkt über ApiConfig.BaseUrl zu.
public static class ApiConfig
{
    // WICHTIG: Wir verwenden DIREKT HTTPS (nicht HTTP). Grund: Ruft der Client zuerst HTTP auf, antwortet der
    // Server mit einem Redirect (Umleitung) auf HTTPS. Bei diesem Redirect verwirft der HttpClient aus
    // Sicherheitsgründen den "Authorization: Bearer ..."-Header – der Token ginge also verloren und alle
    // geschützten Endpunkte würden trotz erfolgreichem Login mit 401 antworten. HTTPS direkt vermeidet das.
    //
    // Plattform-Weiche per Präprozessor (#if): Der Android-EMULATOR läuft in einer virtuellen Maschine; aus
    // seiner Sicht ist "localhost" der Emulator selbst, NICHT der Entwickler-PC. Die Spezial-Adresse 10.0.2.2
    // ist im Android-Emulator fest auf den Host-Rechner gemappt. Andere Plattformen erreichen den Server über localhost.
#if ANDROID
    public const string BaseUrl = "https://10.0.2.2:7221";
#else
    public const string BaseUrl = "https://localhost:7221";
#endif

    // Bequeme Umwandlung des Strings in ein Uri-Objekt; so wird der HttpClient in MauiProgram initialisiert.
    public static Uri BaseUri => new(BaseUrl);
}
