using Brokey_APP.Models;

namespace Brokey_APP.Services;

// Vertrag (Interface) für alle Trip-, Gruppen-, Mitglieder-, Ausgaben- und Abrechnungs-Funktionen.
// Implementiert von TripService; genutzt von TripsViewModel, TripDetailViewModel, GroupDetailViewModel,
// AddExpenseViewModel, ExpenseDetailViewModel u. a. Jede Methode entspricht genau einem API-Endpunkt.
public interface ITripService
{
    // Liste aller Trips des eingeloggten Nutzers.
    Task<IReadOnlyList<TripSummaryResponse>> GetTripsAsync();
    // Die 10 neuesten Ausgaben über alle Trips (für die Startseite).
    Task<IReadOnlyList<ExpenseResponse>> GetRecentActivitiesAsync();
    // Legt einen neuen Trip an.
    Task<TripDetailResponse> CreateTripAsync(CreateTripRequest request);
    // Holt die Details eines Trips (inkl. Gruppen und Mitglieder).
    Task<TripDetailResponse> GetTripAsync(int tripId);
    // Liste der Gruppen eines Trips.
    Task<IReadOnlyList<GroupResponse>> GetGroupsAsync(int tripId);
    // Legt eine neue Gruppe innerhalb eines Trips an.
    Task<GroupResponse> CreateGroupAsync(int tripId, CreateGroupRequest request);
    // Liste der Mitglieder einer Gruppe.
    Task<IReadOnlyList<GroupMemberResponse>> GetGroupMembersAsync(int groupId);
    // Fügt einer Gruppe ein Mitglied hinzu (per Username oder E-Mail).
    Task<GroupMemberResponse> AddGroupMemberAsync(int groupId, AddGroupMemberRequest request);
    // Entfernt ein Mitglied aus einer Gruppe.
    Task RemoveGroupMemberAsync(int groupId, int userId);
    // Liste der verfügbaren Ausgaben-Kategorien einer Gruppe (z. B. Essen, Transport).
    Task<IReadOnlyList<ExpenseCategoryResponse>> GetExpenseCategoriesAsync(int groupId);
    // Liste der Ausgaben einer Gruppe.
    Task<IReadOnlyList<ExpenseResponse>> GetGroupExpensesAsync(int groupId);
    // Eine einzelne Ausgabe einer Gruppe (Detailansicht).
    Task<ExpenseResponse> GetGroupExpenseByIdAsync(int groupId, int expenseId);
    // Legt eine neue Ausgabe in einer Gruppe an.
    Task<ExpenseResponse> CreateExpenseAsync(int groupId, CreateExpenseRequest request);
    // Aktualisiert eine bestehende Ausgabe.
    Task<ExpenseResponse> UpdateExpenseAsync(int groupId, int expenseId, CreateExpenseRequest request);
    // Löscht eine Ausgabe.
    Task DeleteExpenseAsync(int groupId, int expenseId);
    // Berechnet/holt die Abrechnung einer Gruppe (wer schuldet wem wie viel).
    Task<GroupSettlementResponse> GetGroupSettlementAsync(int groupId);
}
