using System.Net.Http.Json;
using System.Text.Json;
using Brokey_APP.Models;

namespace Brokey_APP.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorageService _tokenStorageService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthService(HttpClient httpClient, ITokenStorageService tokenStorageService)
    {
        _httpClient = httpClient;
        _tokenStorageService = tokenStorageService;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var error = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, JsonOptions);
                throw new Exception(error?.Message ?? "Login failed.");
            }
            catch (JsonException)
            {
                throw new Exception($"Login failed. (HTTP {(int)response.StatusCode})");
            }
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions)
                           ?? throw new Exception("Invalid server response.");

        await _tokenStorageService.SaveTokenAsync(authResponse.Token);
        return authResponse;
    }

    public async Task<AuthResponse> RegisterAsync(string username, string email, string password, string baseCurrency)
    {
        var request = new RegisterRequest
        {
            Username = username,
            Email = email,
            Password = password,
            BaseCurrency = baseCurrency
        };

        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

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

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions)
                           ?? throw new Exception("Invalid server response.");

        await _tokenStorageService.SaveTokenAsync(authResponse.Token);
        return authResponse;
    }

    public async Task<UserResponse> GetCurrentUserAsync()
    {
        var response = await _httpClient.GetAsync("api/auth/me");

        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to fetch user profile.");

        return await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions)
               ?? throw new Exception("Invalid server response.");
    }

    public Task LogoutAsync()
    {
        return _tokenStorageService.ClearTokenAsync();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _tokenStorageService.GetTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }
}
