using System.Net.Http.Headers;

namespace Brokey_APP.Services;

public class AuthHttpMessageHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorageService;

    public AuthHttpMessageHandler(ITokenStorageService tokenStorageService)
    {
        _tokenStorageService = tokenStorageService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        var isAuthRequest = path.EndsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
                            path.EndsWith("/api/auth/register", StringComparison.OrdinalIgnoreCase);

        if (!isAuthRequest)
        {
            var token = await _tokenStorageService.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
