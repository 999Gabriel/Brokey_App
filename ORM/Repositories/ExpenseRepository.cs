using Microsoft.EntityFrameworkCore;
using Models;

namespace ORM.Repositories;

public class ExpenseRepository
{
    private readonly AppDbContext _context;

    public ExpenseRepository(AppDbContext context)
    {
        _context = context;
    }

    // Gibt alle Expense-Kategorien alphabetisch sortiert zurück (aus vorgeseederter Tabelle).
    // → GetExpenseCategories-Endpoint → AddExpenseViewModel.Categories.
    public async Task<List<ExpenseCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ExpenseCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    // Lädt alle Ausgaben einer Gruppe mit allen Navigation-Properties (Category, PaidBy, Splits, User).
    // Sortiert: neuestes Datum zuerst, bei Gleichstand nach Erstellungszeit.
    public async Task<List<Expense>> GetGroupExpensesAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await BuildExpenseProjectionQuery()
            .AsNoTracking()
            .Where(e => e.GroupId == groupId)
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // Lädt eine einzelne Ausgabe anhand von Group- und Expense-ID inkl. aller Navigation-Properties.
    // → ExpenseDetailViewModel, AddExpenseViewModel (Edit-Modus).
    public async Task<Expense?> GetGroupExpenseByIdAsync(
        int groupId,
        int expenseId,
        CancellationToken cancellationToken = default)
    {
        return await BuildExpenseProjectionQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.GroupId == groupId && e.Id == expenseId, cancellationToken);
    }

    // Erstellt eine neue Ausgabe inkl. ExpenseSplit-Einträge in einer DB-Transaktion.
    // Berechnet die Split-Beträge über BuildSplitEntities, speichert die Ausgabe und liest sie neu ein.
    // Gibt null zurück, wenn keine Teilnehmer vorhanden sind.
    public async Task<Expense?> CreateGroupExpenseAsync(
        int tripId,
        int groupId,
        int paidByUserId,
        int categoryId,
        string title,
        string? description,
        decimal amount,
        DateTime expenseDate,
        decimal? latitude,
        decimal? longitude,
        IReadOnlyCollection<int> splitUserIds,
        IReadOnlyDictionary<int, decimal>? splitAmountsByUser,
        CancellationToken cancellationToken = default)
    {
        var splits = BuildSplitEntities(amount, splitUserIds, splitAmountsByUser);
        if (splits.Count == 0)
        {
            return null;
        }

        var expense = new Expense
        {
            TripId = tripId,
            GroupId = groupId,
            PaidByUserId = paidByUserId,
            CategoryId = categoryId,
            Title = title,
            Description = description,
            Amount = amount,
            Latitude = latitude,
            Longitude = longitude,
            ExpenseDate = expenseDate,
            CreatedAt = DateTime.UtcNow,
            Splits = splits
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetGroupExpenseByIdAsync(groupId, expense.Id, cancellationToken);
    }

    // Aktualisiert eine bestehende Ausgabe: löscht alle alten Splits und erstellt neue.
    // Gibt null zurück, wenn die Ausgabe nicht gefunden wurde oder keine Teilnehmer gesetzt sind.
    public async Task<Expense?> UpdateGroupExpenseAsync(
        int groupId,
        int expenseId,
        int paidByUserId,
        int categoryId,
        string title,
        string? description,
        decimal amount,
        DateTime expenseDate,
        decimal? latitude,
        decimal? longitude,
        IReadOnlyCollection<int> splitUserIds,
        IReadOnlyDictionary<int, decimal>? splitAmountsByUser,
        CancellationToken cancellationToken = default)
    {
        var expense = await _context.Expenses
            .Include(e => e.Splits)
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.GroupId == groupId, cancellationToken);

        if (expense == null)
        {
            return null;
        }

        var splits = BuildSplitEntities(amount, splitUserIds, splitAmountsByUser);
        if (splits.Count == 0)
        {
            return null;
        }

        expense.PaidByUserId = paidByUserId;
        expense.CategoryId = categoryId;
        expense.Title = title;
        expense.Description = description;
        expense.Amount = amount;
        expense.ExpenseDate = expenseDate;
        expense.Latitude = latitude;
        expense.Longitude = longitude;

        _context.ExpenseSplits.RemoveRange(expense.Splits);
        expense.Splits = splits;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetGroupExpenseByIdAsync(groupId, expense.Id, cancellationToken);
    }

    // Löscht eine Ausgabe; durch Cascade in der DB werden auch alle zugehörigen Splits gelöscht.
    public async Task<bool> DeleteGroupExpenseAsync(
        int groupId,
        int expenseId,
        CancellationToken cancellationToken = default)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.GroupId == groupId, cancellationToken);

        if (expense == null)
        {
            return false;
        }

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Markiert alle offenen Splits zwischen zwei Usern (fromUser schuldet toUser) als bezahlt (IsSettled=true).
    // Gibt die Anzahl der markierten Splits zurück; 0 wenn keine offenen Schulden gefunden wurden.
    public async Task<int> MarkSplitSettledAsync(
        int groupId,
        int fromUserId,
        int toUserId,
        CancellationToken cancellationToken = default)
    {
        var unsettledSplits = await _context.ExpenseSplits
            .Include(split => split.Expense)
            .Where(split =>
                split.UserId == fromUserId &&
                !split.IsSettled &&
                split.UserId != split.Expense.PaidByUserId &&
                split.Expense.GroupId == groupId &&
                split.Expense.PaidByUserId == toUserId)
            .ToListAsync(cancellationToken);

        if (unsettledSplits.Count == 0)
        {
            return 0;
        }

        var settledAt = DateTime.UtcNow;
        foreach (var split in unsettledSplits)
        {
            split.IsSettled = true;
            split.SettledAt = settledAt;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return unsettledSplits.Count;
    }

    // Gibt die neuesten `count` Ausgaben aller Trips zurück, an denen der User beteiligt ist.
    // → TripsController.GetRecentActivities → HomeViewModel.RecentActivities.
    public async Task<List<Expense>> GetRecentExpensesForUserAsync(int userId, int count = 10, CancellationToken cancellationToken = default)
    {
        return await BuildExpenseProjectionQuery()
            .AsNoTracking()
            .Where(e => e.Trip.Members.Any(m => m.UserId == userId))
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    // Basis-Query mit allen nötigen Includes (Trip, Group, PaidBy, Category, Splits+User).
    // Wird von GetGroupExpensesAsync, GetGroupExpenseByIdAsync und GetRecentExpensesForUserAsync wiederverwendet.
    private IQueryable<Expense> BuildExpenseProjectionQuery()
    {
        return _context.Expenses
            .Include(e => e.Trip)
            .Include(e => e.Group)
                .ThenInclude(g => g!.Trip)
            .Include(e => e.PaidBy)
            .Include(e => e.Category)
            .Include(e => e.Splits)
                .ThenInclude(s => s.User);
    }

    // Erstellt die ExpenseSplit-Objekte für eine Ausgabe.
    // Ohne splitAmountsByUser: gleiche Aufteilung per CalculateSplitAmounts.
    // Mit splitAmountsByUser: direkte Zuweisung der vorberechneten Beträge + Rundungskorrektur.
    private static List<ExpenseSplit> BuildSplitEntities(
        decimal amount,
        IReadOnlyCollection<int> splitUserIds,
        IReadOnlyDictionary<int, decimal>? splitAmountsByUser)
    {
        var participantIds = (splitAmountsByUser == null
                ? splitUserIds
                : splitAmountsByUser.Keys)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        if (participantIds.Count == 0)
        {
            return [];
        }

        if (splitAmountsByUser == null)
        {
            var splitAmounts = CalculateSplitAmounts(amount, participantIds.Count);
            return participantIds
                .Select((userId, index) => new ExpenseSplit
                {
                    UserId = userId,
                    Amount = splitAmounts[index],
                    IsSettled = false
                })
                .ToList();
        }

        var normalizedAllocations = participantIds
            .Select(userId => (
                UserId: userId,
                Amount: decimal.Round(splitAmountsByUser[userId], 2, MidpointRounding.AwayFromZero)))
            .ToList();

        var distributed = normalizedAllocations.Sum(a => a.Amount);
        var difference = decimal.Round(amount - distributed, 2, MidpointRounding.AwayFromZero);
        if (difference != 0 && normalizedAllocations.Count > 0)
        {
            var first = normalizedAllocations[0];
            normalizedAllocations[0] = (first.UserId, first.Amount + difference);
        }

        return normalizedAllocations
            .Select(allocation => new ExpenseSplit
            {
                UserId = allocation.UserId,
                Amount = allocation.Amount,
                IsSettled = false
            })
            .ToList();
    }

    // Berechnet gleiche Anteile (totalAmount / n) und fügt eine etwaige Cent-Differenz beim ersten Teilnehmer hinzu.
    // Beispiel: 10€ / 3 = [3.34, 3.33, 3.33].
    private static List<decimal> CalculateSplitAmounts(decimal totalAmount, int participantCount)
    {
        var safeParticipantCount = Math.Max(1, participantCount);
        var baseShare = decimal.Round(totalAmount / safeParticipantCount, 2, MidpointRounding.AwayFromZero);

        var result = Enumerable.Repeat(baseShare, safeParticipantCount).ToList();
        var distributed = baseShare * safeParticipantCount;
        var remainder = totalAmount - distributed;

        if (remainder != 0)
        {
            result[0] += remainder;
        }

        return result;
    }
}
