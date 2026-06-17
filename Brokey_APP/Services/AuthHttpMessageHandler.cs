using System.Net.Http.Headers;   // AuthenticationHeaderValue zum Aufbau des "Authorization: Bearer ..."-Headers.

namespace Brokey_APP.Services;

// Ein DelegatingHandler ist ein "Zwischenschicht"-Glied in der HttpClient-Pipeline: jeder ausgehende
// Request läuft durch SendAsync, bevor er wirklich ins Netz geht. Diese Klasse nutzt das, um bei JEDEM
// Request automatisch den gespeicherten JWT als "Authorization: Bearer <token>"-Header einzufügen –
// AUSSER bei Login/Register (dort gibt es ja noch keinen Token). So müssen die Services den Token nie
// selbst anhängen. Registriert in MauiProgram für beide HttpClients (Auth- und Trip-Client).
public class AuthHttpMessageHandler : DelegatingHandler
{
    // Quelle des Tokens: der dreistufige Token-Speicher (Memory -> Preferences -> SecureStorage).
    private readonly ITokenStorageService _tokenStorageService;

    public AuthHttpMessageHandler(ITokenStorageService tokenStorageService)
    {
        _tokenStorageService = tokenStorageService;
    }

    // Wird von der HttpClient-Pipeline für jeden ausgehenden Request automatisch aufgerufen.
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Pfad der Ziel-URL ermitteln (z. B. "/api/trips"), um Login/Register zu erkennen.
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        // Login- und Register-Requests dürfen KEINEN Bearer-Header bekommen (es gibt noch keinen gültigen Token).
        var isAuthRequest = path.EndsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
                            path.EndsWith("/api/auth/register", StringComparison.OrdinalIgnoreCase);

        if (!isAuthRequest)
        {
            // Für alle anderen (geschützten) Requests den Token holen ...
            var token = await _tokenStorageService.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                // ... und als Authorization-Header setzen, damit die API den Nutzer authentifizieren kann.
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // Den (ggf. um den Header ergänzten) Request an das nächste Pipeline-Glied weiterreichen und tatsächlich senden.
        return await base.SendAsync(request, cancellationToken);
    }
}
