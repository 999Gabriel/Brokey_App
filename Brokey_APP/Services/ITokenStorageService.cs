namespace Brokey_APP.Services;

public interface ITokenStorageService
{
    Task SaveTokenAsync(string token);
    Task<string?> GetTokenAsync();
    Task ClearTokenAsync();
    bool HasStoredToken();
}
