namespace Brokey_APP.Services;

// Service-Klasse, die den JWT (Login-Token) auf dem Gerät verwaltet: speichern, lesen, löschen.
// Sie nutzt einen DREISTUFIGEN Speicher, um Geschwindigkeit, Persistenz und Sicherheit zu kombinieren:
//   Stufe 1 (Memory-Cache, _cachedToken): blitzschnell, aber nur solange die App läuft.
//   Stufe 2 (Preferences):                einfacher persistenter Schlüssel-Wert-Speicher, überlebt App-Neustarts.
//   Stufe 3 (SecureStorage):              verschlüsselter Geräte-Speicher (Keychain/Keystore), am sichersten,
//                                         aber auf manchen Plattformen/Emulatoren nicht verfügbar -> daher abgesichert.
// Implementiert ITokenStorageService; genutzt von AuthService, TripService und AuthHttpMessageHandler.
public class TokenStorageService : ITokenStorageService
{
    // Einheitlicher Schlüsselname, unter dem der Token in allen drei Speichern abgelegt wird.
    private const string TokenKey = "auth_token";
    // Stufe 1: In-Memory-Kopie des Tokens für schnellen Zugriff (null = aktuell kein Token bekannt).
    private string? _cachedToken;

    // Konstruktor: liest beim Erstellen des Service den eventuell schon gespeicherten Token aus Preferences
    // in den Memory-Cache, damit ein bereits eingeloggter Nutzer nach App-Neustart sofort erkannt wird.
    public TokenStorageService()
    {
        _cachedToken = Preferences.Default.Get(TokenKey, default(string));
    }

    // Speichert den JWT in ALLEN Stufen: erst Memory, dann Preferences (persistent), dann – wenn möglich – SecureStorage.
    public async Task SaveTokenAsync(string token)
    {
        // Stufe 1: Memory-Cache sofort setzen.
        _cachedToken = token;
        // Stufe 2: Preferences setzen (überlebt App-Neustart, auch wenn SecureStorage scheitert).
        Preferences.Default.Set(TokenKey, token);

        try
        {
            // Stufe 3: verschlüsselt ablegen (bester Schutz). Kann auf manchen Plattformen fehlschlagen.
            await SecureStorage.Default.SetAsync(TokenKey, token);
        }
        catch
        {
            // Schlägt das verschlüsselte Speichern fehl, versuchen wir einen evtl. veralteten Eintrag aufzuräumen,
            // damit später nicht ein falscher (alter) Token aus SecureStorage gelesen wird.
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

    // Liest den JWT entlang der Stufen-Reihenfolge: zuerst Memory (schnellster), dann Preferences, zuletzt SecureStorage.
    // Wird vom AuthHttpMessageHandler bei jedem geschützten HTTP-Request aufgerufen, um den Bearer-Header zu setzen.
    public async Task<string?> GetTokenAsync()
    {
        // Stufe 1: Liegt der Token bereits im Memory-Cache, sofort zurückgeben (kein Speicherzugriff nötig).
        if (!string.IsNullOrWhiteSpace(_cachedToken))
        {
            return _cachedToken;
        }

        // Stufe 2: aus Preferences lesen. Treffer -> in den Memory-Cache übernehmen und zurückgeben.
        var preferenceToken = Preferences.Default.Get(TokenKey, default(string));
        if (!string.IsNullOrWhiteSpace(preferenceToken))
        {
            _cachedToken = preferenceToken;
            return preferenceToken;
        }

        try
        {
            // Stufe 3: als letzten Ausweg den verschlüsselten SecureStorage abfragen.
            var secureToken = await SecureStorage.Default.GetAsync(TokenKey);
            if (!string.IsNullOrWhiteSpace(secureToken))
            {
                // Treffer: in Memory UND Preferences zurückspiegeln, damit die nächsten Zugriffe schneller sind.
                _cachedToken = secureToken;
                Preferences.Default.Set(TokenKey, secureToken);
                return secureToken;
            }
        }
        catch
        {
            // SecureStorage nicht verfügbar -> wir fallen unten auf den (leeren) Preferences-Wert zurück.
        }

        // Kein Token in irgendeiner Stufe gefunden -> gibt den (leeren/null) Preferences-Wert zurück.
        return preferenceToken;
    }

    // Löscht den JWT aus ALLEN drei Stufen. Wird beim Logout und bei einer 401-Antwort (abgelaufene Session) aufgerufen.
    public Task ClearTokenAsync()
    {
        // Stufe 1: Memory-Cache leeren.
        _cachedToken = null;

        try
        {
            // Stufe 3: verschlüsselten Eintrag entfernen (kann auf manchen Plattformen scheitern).
            SecureStorage.Default.Remove(TokenKey);
        }
        catch
        {
            // Ignore unsupported secure storage cleanup failures.
        }

        // Stufe 2: Preferences-Eintrag entfernen.
        Preferences.Default.Remove(TokenKey);
        // Es gibt keine echte asynchrone Arbeit -> abgeschlossenen Task zurückgeben (Methode bleibt zur Vertragstreue Task-basiert).
        return Task.CompletedTask;
    }

    // Schnelle SYNCHRONE Prüfung (ohne async/await), ob irgendwo ein Token liegt – ohne den teureren SecureStorage anzufassen.
    // Wird beim App-Start für die initiale Shell-Entscheidung (Login-Oberfläche vs. App-Oberfläche) verwendet.
    public bool HasStoredToken()
    {
        return !string.IsNullOrWhiteSpace(_cachedToken) ||
               !string.IsNullOrWhiteSpace(Preferences.Default.Get(TokenKey, default(string)));
    }
}
