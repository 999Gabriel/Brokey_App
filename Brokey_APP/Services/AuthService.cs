using System.Net.Http.Json;   // Erweiterungsmethoden wie PostAsJsonAsync / ReadFromJsonAsync (Objekt <-> JSON in einem Schritt).
using System.Text.Json;        // JsonSerializer + JsonSerializerOptions für manuelles (De-)Serialisieren von Fehler-Bodies.
using Brokey_APP.Models;       // Die Request-/Response-Datenklassen (LoginRequest, AuthResponse, ...).

namespace Brokey_APP.Services;

// Service-Klasse, die die GESAMTE Authentifizierungs-Kommunikation mit dem API_Server kapselt
// (Login, Registrierung, eigenes Profil laden, Logout). Implementiert IAuthService, damit die
// ViewModels nur gegen das Interface programmieren und der DI-Container die konkrete Klasse einsetzt.
// Datenfluss: ViewModel -> AuthService (HTTP-Request) -> API -> DB -> JSON-Antwort -> Model-Objekt -> ViewModel.
public class AuthService : IAuthService
{
    // Der HttpClient wird vom DI-Container injiziert; seine BaseUrl ist bereits auf ApiConfig.BaseUrl gesetzt.
    private readonly HttpClient _httpClient;
    // Verantwortlich fürs Speichern/Lesen/Löschen des JWT (dreistufiger Speicher). Wird ebenfalls injiziert.
    private readonly ITokenStorageService _tokenStorageService;

    // Optionen fürs JSON-Parsen: Property-Namen werden ohne Rücksicht auf Groß-/Kleinschreibung gematcht,
    // damit z. B. das API-Feld "token" auch auf die C#-Property "Token" passt.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Konstruktor: Der DI-Container reicht HttpClient und Token-Speicher herein (Constructor Injection).
    public AuthService(HttpClient httpClient, ITokenStorageService tokenStorageService)
    {
        _httpClient = httpClient;
        _tokenStorageService = tokenStorageService;
    }

    // POST /api/auth/login → löscht alten Token, schickt Credentials, speichert neuen JWT via TokenStorageService.
    // Wirft Exception mit der Server-Fehlermeldung bei fehlgeschlagenem Login. Aufgerufen von LoginViewModel.
    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        // 1) Eingaben in das Request-Objekt packen, das genau dem JSON-Body der API entspricht.
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        // 2) Eventuell noch vorhandenen alten Token entfernen, damit kein ungültiger Bearer-Header mitgeschickt wird.
        await _tokenStorageService.ClearTokenAsync();
        // 3) Request-Objekt als JSON serialisieren und per HTTP POST an /api/auth/login senden.
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

        // 4) Bei einem Fehler-Statuscode (z. B. 401 falsche Daten) versuchen wir, die Server-Fehlermeldung herauszulesen.
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                // Body als ApiErrorResponse deuten und dessen Message als Exception werfen.
                var error = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, JsonOptions);
                throw new Exception(error?.Message ?? "Login failed.");
            }
            catch (JsonException)
            {
                // War der Body kein gültiges JSON, geben wir wenigstens den HTTP-Statuscode aus.
                throw new Exception($"Login failed. (HTTP {(int)response.StatusCode})");
            }
        }

        // 5) Erfolgsfall: JSON-Antwort in ein AuthResponse-Objekt deserialisieren (null => ungültige Antwort).
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions)
                           ?? throw new Exception("Invalid server response.");

        // 6) Sicherheitscheck: ohne Token wäre der Login wertlos (alle Folge-Requests bräuchten ihn).
        if (string.IsNullOrWhiteSpace(authResponse.Token))
        {
            throw new Exception("Login succeeded, but the server did not return a valid token.");
        }

        // 7) JWT im dreistufigen Speicher ablegen, damit ihn der AuthHttpMessageHandler künftig automatisch anhängt.
        await _tokenStorageService.SaveTokenAsync(authResponse.Token);
        return authResponse;
    }

    // POST /api/auth/register → löscht alten Token, schickt Registrierungsdaten, speichert neuen JWT.
    // Identischer Ablauf wie LoginAsync, nur mit zusätzlichen Feldern (Username, BaseCurrency). Aufgerufen von RegisterViewModel.
    public async Task<AuthResponse> RegisterAsync(string username, string email, string password, string baseCurrency)
    {
        // Registrierungsdaten in das Request-Objekt füllen (entspricht dem JSON-Body der API).
        var request = new RegisterRequest
        {
            Username = username,
            Email = email,
            Password = password,
            BaseCurrency = baseCurrency
        };

        // Alten Token entfernen, dann Daten per POST an die Register-Route senden.
        await _tokenStorageService.ClearTokenAsync();
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

        // Fehlerfall: Server-Fehlermeldung extrahieren (z. B. "E-Mail bereits vergeben") und als Exception werfen.
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var error = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, JsonOptions);
                throw new Exception(error?.Message ?? "Registration failed.");
            }
            catch (JsonException)
            {
                throw new Exception($"Registration failed. (HTTP {(int)response.StatusCode})");
            }
        }

        // Erfolgsfall: Antwort deserialisieren ...
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions)
                           ?? throw new Exception("Invalid server response.");

        // ... Token-Vorhandensein prüfen ...
        if (string.IsNullOrWhiteSpace(authResponse.Token))
        {
            throw new Exception("Registration succeeded, but the server did not return a valid token.");
        }

        // ... und den neuen JWT speichern, sodass der Nutzer direkt nach der Registrierung eingeloggt ist.
        await _tokenStorageService.SaveTokenAsync(authResponse.Token);
        return authResponse;
    }

    // GET /api/auth/me → gibt das UserResponse des eingeloggten Users zurück (Username, E-Mail, Basiswährung, ...).
    // Der JWT wird automatisch vom AuthHttpMessageHandler angehängt. Wird von HomeViewModel und ProfileViewModel beim Laden aufgerufen.
    public async Task<UserResponse> GetCurrentUserAsync()
    {
        // HTTP GET ohne Body; die API erkennt den Nutzer am mitgeschickten Bearer-Token.
        var response = await _httpClient.GetAsync("api/auth/me");

        // Bei Fehler (z. B. Token abgelaufen => 401) wird eine generische Exception geworfen.
        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to fetch user profile.");

        // Erfolgsfall: JSON in das UserResponse-Model umwandeln und zurückgeben.
        return await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions)
               ?? throw new Exception("Invalid server response.");
    }

    // Logout: löscht nur den lokal gespeicherten JWT. Es ist KEIN HTTP-Request nötig, da JWTs stateless sind
    // (der Server hält keine Session; ohne Token kommt man einfach nicht mehr durch). Aufgerufen von ProfileViewModel.
    public Task LogoutAsync()
    {
        return _tokenStorageService.ClearTokenAsync();
    }

    // Prüft, OB überhaupt ein JWT gespeichert ist (ohne dessen Ablaufdatum/Signatur zu validieren).
    // Wird beim App-Start genutzt, um zu entscheiden, ob direkt die App-Oberfläche statt der Login-Seite gezeigt wird.
    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _tokenStorageService.GetTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }
}
