using System.Net.Http.Json;
using System.Text.Json;
using Brokey_APP.Models;

namespace Brokey_APP.Services;

public class TripService : ITripService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TripService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<TripSummaryResponse>> GetTripsAsync()
    {
        var response = await _httpClient.GetAsync("api/trips");
        await EnsureSuccessAsync(response, "Failed to load trips.");

        return await response.Content.ReadFromJsonAsync<List<TripSummaryResponse>>(JsonOptions) ?? [];
    }

    public async Task<TripDetailResponse> CreateTripAsync(CreateTripRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/trips", request);
        await EnsureSuccessAsync(response, "Failed to create trip.");
        return await ReadRequiredAsync<TripDetailResponse>(response);
    }

    public async Task<TripDetailResponse> GetTripAsync(int tripId)
    {
        var response = await _httpClient.GetAsync($"api/trips/{tripId}");
        await EnsureSuccessAsync(response, "Failed to load trip.");
        return await ReadRequiredAsync<TripDetailResponse>(response);
    }

    public async Task<IReadOnlyList<GroupResponse>> GetGroupsAsync(int tripId)
    {
        var response = await _httpClient.GetAsync($"api/trips/{tripId}/groups");
        await EnsureSuccessAsync(response, "Failed to load groups.");

        return await response.Content.ReadFromJsonAsync<List<GroupResponse>>(JsonOptions) ?? [];
    }

    public async Task<GroupResponse> CreateGroupAsync(int tripId, CreateGroupRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/trips/{tripId}/groups", request);
        await EnsureSuccessAsync(response, "Failed to create group.");
        return await ReadRequiredAsync<GroupResponse>(response);
    }

    public async Task<IReadOnlyList<GroupMemberResponse>> GetGroupMembersAsync(int groupId)
    {
        var response = await _httpClient.GetAsync($"api/groups/{groupId}/members");
        await EnsureSuccessAsync(response, "Failed to load group members.");

        return await response.Content.ReadFromJsonAsync<List<GroupMemberResponse>>(JsonOptions) ?? [];
    }

    public async Task<GroupMemberResponse> AddGroupMemberAsync(int groupId, AddGroupMemberRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/groups/{groupId}/members", request);
        await EnsureSuccessAsync(response, "Failed to add member.");
        return await ReadRequiredAsync<GroupMemberResponse>(response);
    }

    public async Task RemoveGroupMemberAsync(int groupId, int userId)
    {
        var response = await _httpClient.DeleteAsync($"api/groups/{groupId}/members/{userId}");
        await EnsureSuccessAsync(response, "Failed to remove member.");
    }

    private static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions)
               ?? throw new Exception("Invalid server response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string fallbackMessage)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var errorContent = await response.Content.ReadAsStringAsync();

        try
        {
            var error = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, JsonOptions);
            throw new Exception(error?.Message ?? fallbackMessage);
        }
        catch (JsonException)
        {
            throw new Exception(fallbackMessage);
        }
    }
}
