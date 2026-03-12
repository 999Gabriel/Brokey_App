using Brokey_APP.Models;

namespace Brokey_APP.Services;

public interface ITripService
{
    Task<IReadOnlyList<TripSummaryResponse>> GetTripsAsync();
    Task<TripDetailResponse> CreateTripAsync(CreateTripRequest request);
    Task<TripDetailResponse> GetTripAsync(int tripId);
    Task<IReadOnlyList<GroupResponse>> GetGroupsAsync(int tripId);
    Task<GroupResponse> CreateGroupAsync(int tripId, CreateGroupRequest request);
    Task<IReadOnlyList<GroupMemberResponse>> GetGroupMembersAsync(int groupId);
    Task<GroupMemberResponse> AddGroupMemberAsync(int groupId, AddGroupMemberRequest request);
    Task RemoveGroupMemberAsync(int groupId, int userId);
}
