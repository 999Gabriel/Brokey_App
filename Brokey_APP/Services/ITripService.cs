using Brokey_APP.Models;

namespace Brokey_APP.Services;

public interface ITripService
{
    Task<IReadOnlyList<TripSummaryResponse>> GetTripsAsync();
    Task<IReadOnlyList<ExpenseResponse>> GetRecentActivitiesAsync();
    Task<TripDetailResponse> CreateTripAsync(CreateTripRequest request);
    Task<TripDetailResponse> GetTripAsync(int tripId);
    Task<IReadOnlyList<GroupResponse>> GetGroupsAsync(int tripId);
    Task<GroupResponse> CreateGroupAsync(int tripId, CreateGroupRequest request);
    Task<IReadOnlyList<GroupMemberResponse>> GetGroupMembersAsync(int groupId);
    Task<GroupMemberResponse> AddGroupMemberAsync(int groupId, AddGroupMemberRequest request);
    Task RemoveGroupMemberAsync(int groupId, int userId);
    Task<IReadOnlyList<ExpenseCategoryResponse>> GetExpenseCategoriesAsync(int groupId);
    Task<IReadOnlyList<ExpenseResponse>> GetGroupExpensesAsync(int groupId);
    Task<ExpenseResponse> GetGroupExpenseByIdAsync(int groupId, int expenseId);
    Task<ExpenseResponse> CreateExpenseAsync(int groupId, CreateExpenseRequest request);
    Task<ExpenseResponse> UpdateExpenseAsync(int groupId, int expenseId, CreateExpenseRequest request);
    Task DeleteExpenseAsync(int groupId, int expenseId);
    Task<GroupSettlementResponse> GetGroupSettlementAsync(int groupId);
}
