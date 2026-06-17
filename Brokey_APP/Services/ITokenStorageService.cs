namespace Brokey_APP.Services;

// Vertrag (Interface) für die Token-Verwaltung auf dem Gerät. Implementiert von TokenStorageService
// (dreistufiger Speicher: Memory -> Preferences -> SecureStorage). Genutzt von AuthService, TripService
// und AuthHttpMessageHandler – alle arbeiten nur gegen diese Abstraktion.
public interface ITokenStorageService
{
    // Speichert den JWT (in allen Speicher-Stufen).
    Task SaveTokenAsync(string token);
    // Liest den JWT (null/leer, wenn keiner vorhanden ist).
    Task<string?> GetTokenAsync();
    // Löscht den JWT überall (Logout / abgelaufene Session).
    Task ClearTokenAsync();
    // Schnelle synchrone Ja/Nein-Prüfung, ob ein Token gespeichert ist (für den App-Start).
    bool HasStoredToken();
}
