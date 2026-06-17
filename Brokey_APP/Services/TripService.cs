using System.Net.Http.Json;   // PostAsJsonAsync / PutAsJsonAsync / ReadFromJsonAsync (Objekt <-> JSON).
using System.Text.Json;        // JsonSerializer für das Parsen von Fehler-Bodies.
using Brokey_APP.Models;       // Alle Trip-/Group-/Expense-/Settlement-Datenklassen.

namespace Brokey_APP.Services;

// Service-Klasse, die die GESAMTE HTTP-Kommunikation rund um Trips, Gruppen, Mitglieder, Ausgaben und
// Abrechnungen (Settlement) kapselt. Implementiert ITripService, damit die ViewModels gegen die
// Abstraktion arbeiten. Jede Methode entspricht genau einem API-Endpunkt; die JSON-Antworten werden
// in die Model-Klassen deserialisiert. Der JWT wird automatisch vom AuthHttpMessageHandler angehängt.
public class TripService : ITripService
{
    // Vom DI-Container injizierter HttpClient mit gesetzter BaseUrl (ApiConfig.BaseUrl).
    private readonly HttpClient _httpClient;
    // Token-Speicher: wird hier nur gebraucht, um den JWT bei einer 401-Antwort zu löschen.
    private readonly ITokenStorageService _tokenStorageService;

    // Gemeinsame JSON-Optionen: Groß-/Kleinschreibung der Property-Namen wird ignoriert.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Constructor Injection: HttpClient und Token-Speicher werden vom DI-Container bereitgestellt.
    public TripService(HttpClient httpClient, ITokenStorageService tokenStorageService)
    {
        _httpClient = httpClient;
        _tokenStorageService = tokenStorageService;
    }

    // GET /api/trips → gibt alle Trips des eingeloggten Users zurück. → TripsViewModel.
    // Diese Methode zeigt das Standard-Muster aller GET-Listen-Methoden hier:
    //   1) HTTP GET absetzen, 2) Fehler/401 zentral prüfen, 3) JSON-Array in eine Liste deserialisieren.
    public async Task<IReadOnlyList<TripSummaryResponse>> GetTripsAsync()
    {
        // 1) Anfrage an die API; der Bearer-Token wird vom AuthHttpMessageHandler automatisch ergänzt.
        var response = await _httpClient.GetAsync("api/trips");
        // 2) Wirft bei Misserfolg eine Exception; bei 401 wird zusätzlich der Token gelöscht.
        await EnsureSuccessAsync(response, "Failed to load trips.");

        // 3) Body in eine Liste umwandeln; bei null (leerer Body) eine leere Liste "[]" zurückgeben.
        return await response.Content.ReadFromJsonAsync<List<TripSummaryResponse>>(JsonOptions) ?? [];
    }

    // GET /api/trips/recent-activities → 10 neueste Ausgaben aller eigenen Trips. → HomeViewModel.
    public async Task<IReadOnlyList<ExpenseResponse>> GetRecentActivitiesAsync()
    {
        var response = await _httpClient.GetAsync("api/trips/recent-activities");
        await EnsureSuccessAsync(response, "Failed to load recent activities.");

        return await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>(JsonOptions) ?? [];
    }

    // POST /api/trips → erstellt einen neuen Trip und gibt das TripDetailResponse zurück. → CreateTripViewModel.
    public async Task<TripDetailResponse> CreateTripAsync(CreateTripRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/trips", request);
        await EnsureSuccessAsync(response, "Failed to create trip.");
        return await ReadRequiredAsync<TripDetailResponse>(response);
    }

    // GET /api/trips/{tripId} → lädt Trip-Details mit Groups und Members. → TripDetailViewModel, TripSummaryViewModel.
    public async Task<TripDetailResponse> GetTripAsync(int tripId)
    {
        var response = await _httpClient.GetAsync($"api/trips/{tripId}");
        await EnsureSuccessAsync(response, "Failed to load trip.");
        return await ReadRequiredAsync<TripDetailResponse>(response);
    }

    // GET /api/trips/{tripId}/groups → Gruppen-Liste eines Trips. → TripDetailViewModel.
    public async Task<IReadOnlyList<GroupResponse>> GetGroupsAsync(int tripId)
    {
        var response = await _httpClient.GetAsync($"api/trips/{tripId}/groups");
        await EnsureSuccessAsync(response, "Failed to load groups.");

        return await response.Content.ReadFromJsonAsync<List<GroupResponse>>(JsonOptions) ?? [];
    }

    // POST /api/trips/{tripId}/groups → erstellt eine Gruppe. → TripDetailViewModel.CreateGroupAsync.
    public async Task<GroupResponse> CreateGroupAsync(int tripId, CreateGroupRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/trips/{tripId}/groups", request);
        await EnsureSuccessAsync(response, "Failed to create group.");
        return await ReadRequiredAsync<GroupResponse>(response);
    }

    // GET /api/groups/{groupId}/members → Mitglieder-Liste einer Gruppe. → GroupDetailViewModel, AddExpenseViewModel.
    public async Task<IReadOnlyList<GroupMemberResponse>> GetGroupMembersAsync(int groupId)
    {
        var response = await _httpClient.GetAsync($"api/groups/{groupId}/members");
        await EnsureSuccessAsync(response, "Failed to load group members.");

        return await response.Content.ReadFromJsonAsync<List<GroupMemberResponse>>(JsonOptions) ?? [];
    }

    // POST /api/groups/{groupId}/members → fügt Mitglied per Username/Email hinzu. → AddMemberViewModel.
    public async Task<GroupMemberResponse> AddGroupMemberAsync(int groupId, AddGroupMemberRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/groups/{groupId}/members", request);
        await EnsureSuccessAsync(response, "Failed to add member.");
        return await ReadRequiredAsync<GroupMemberResponse>(response);
    }

    // DELETE /api/groups/{groupId}/members/{userId} → entfernt Mitglied. → GroupDetailViewModel.RemoveMemberAsync.
    public async Task RemoveGroupMemberAsync(int groupId, int userId)
    {
        var response = await _httpClient.DeleteAsync($"api/groups/{groupId}/members/{userId}");
        await EnsureSuccessAsync(response, "Failed to remove member.");
    }

    // GET /api/groups/{groupId}/expense-categories → Kategorie-Liste. → AddExpenseViewModel.Categories.
    public async Task<IReadOnlyList<ExpenseCategoryResponse>> GetExpenseCategoriesAsync(int groupId)
    {
        var response = await _httpClient.GetAsync($"api/groups/{groupId}/expense-categories");
        await EnsureSuccessAsync(response, "Failed to load expense categories.");

        return await response.Content.ReadFromJsonAsync<List<ExpenseCategoryResponse>>(JsonOptions) ?? [];
    }

    // GET /api/groups/{groupId}/expenses → Ausgaben-Liste einer Gruppe. → GroupDetailViewModel.Expenses.
    public async Task<IReadOnlyList<ExpenseResponse>> GetGroupExpensesAsync(int groupId)
    {
        var response = await _httpClient.GetAsync($"api/groups/{groupId}/expenses");
        await EnsureSuccessAsync(response, "Failed to load expenses.");

        return await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>(JsonOptions) ?? [];
    }

    // GET /api/groups/{groupId}/expenses/{expenseId} → einzelne Ausgabe. → ExpenseDetailViewModel.
    public async Task<ExpenseResponse> GetGroupExpenseByIdAsync(int groupId, int expenseId)
    {
        var response = await _httpClient.GetAsync($"api/groups/{groupId}/expenses/{expenseId}");
        await EnsureSuccessAsync(response, "Failed to load expense details.");
        return await ReadRequiredAsync<ExpenseResponse>(response);
    }

    // POST /api/groups/{groupId}/expenses → erstellt Ausgabe. → AddExpenseViewModel.SaveExpenseAsync.
    public async Task<ExpenseResponse> CreateExpenseAsync(int groupId, CreateExpenseRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/groups/{groupId}/expenses", request);
        await EnsureSuccessAsync(response, "Failed to create expense.");
        return await ReadRequiredAsync<ExpenseResponse>(response);
    }

    // PUT /api/groups/{groupId}/expenses/{expenseId} → aktualisiert Ausgabe. → AddExpenseViewModel (Edit-Modus).
    public async Task<ExpenseResponse> UpdateExpenseAsync(int groupId, int expenseId, CreateExpenseRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/groups/{groupId}/expenses/{expenseId}", request);
        await EnsureSuccessAsync(response, "Failed to update expense.");
        return await ReadRequiredAsync<ExpenseResponse>(response);
    }

    // DELETE /api/groups/{groupId}/expenses/{expenseId} → löscht Ausgabe. → ExpenseDetailViewModel.DeleteAsync.
    public async Task DeleteExpenseAsync(int groupId, int expenseId)
    {
        var response = await _httpClient.DeleteAsync($"api/groups/{groupId}/expenses/{expenseId}");
        await EnsureSuccessAsync(response, "Failed to delete expense.");
    }

    // GET /api/groups/{groupId}/settlement → Abrechnung mit Balances und Transfers. → GroupDetailViewModel.
    public async Task<GroupSettlementResponse> GetGroupSettlementAsync(int groupId)
    {
        var response = await _httpClient.GetAsync($"api/groups/{groupId}/settlement");
        await EnsureSuccessAsync(response, "Failed to load settlement.");
        return await ReadRequiredAsync<GroupSettlementResponse>(response);
    }

    // Hilfsmethode: deserialisiert den Response-Body in den generischen Typ T und garantiert ein Nicht-null-Ergebnis.
    // Wird von allen Methoden genutzt, die GENAU EIN Objekt (kein Array) zurückgeben, z. B. ein einzelnes ExpenseResponse.
    private static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions)
               ?? throw new Exception("Invalid server response.");
    }

    // Zentrales Fehlerhandling: bei 401 (Unauthorized) wird der Token gelöscht; sonst wird die Server-Fehlermeldung extrahiert.
    // Alle Service-Methoden rufen dies direkt nach dem HTTP-Call auf, um Code-Duplikation zu vermeiden.
    private async Task EnsureSuccessAsync(HttpResponseMessage response, string fallbackMessage)
    {
        // Erfolgs-Statuscode (2xx): nichts zu tun, einfach zurückkehren.
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        // 401 = Token ungültig/abgelaufen: lokal löschen, damit die App den Nutzer zurück zum Login schickt.
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _tokenStorageService.ClearTokenAsync();
            throw new Exception("Your session is no longer valid. Please log in again.");
        }

        // Bei anderen Fehlern (400, 404, 500, ...) versuchen wir, die konkrete Server-Meldung herauszulesen.
        var errorContent = await response.Content.ReadAsStringAsync();

        try
        {
            var error = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, JsonOptions);
            // Server-Message bevorzugen; falls keine vorhanden, die übergebene Fallback-Meldung verwenden.
            throw new Exception(error?.Message ?? fallbackMessage);
        }
        catch (JsonException)
        {
            // War der Body kein JSON, nutzen wir die methodenspezifische Fallback-Meldung.
            throw new Exception(fallbackMessage);
        }
    }
}
