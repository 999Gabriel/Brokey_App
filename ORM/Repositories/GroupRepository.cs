using Microsoft.EntityFrameworkCore;
using Models;

namespace ORM.Repositories;

public class GroupRepository
{
    private readonly AppDbContext _context;

    public GroupRepository(AppDbContext context)
    {
        _context = context;
    }

    // Fügt eine neue Gruppe in die DB ein. → TripsController.CreateGroup ruft dies auf, danach AddMemberAsync.
    public async Task<Group> CreateAsync(Group group, CancellationToken cancellationToken = default)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
        return group;
    }

    // Gibt alle Gruppen eines Trips alphabetisch sortiert zurück inkl. Members und Expenses.
    // → TripsController.GetTripGroups → TripService.GetGroupsAsync.
    public async Task<List<Group>> GetByTripIdAsync(int tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .AsNoTracking()
            .Include(g => g.Members)
            .Include(g => g.Expenses)
            .Where(g => g.TripId == tripId)
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);
    }

    // Lädt eine einzelne Gruppe mit Trip und allen Members (inkl. User-Daten).
    // → GroupsController nutzt dies für fast alle Endpoints.
    public async Task<Group?> GetByIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.Trip)
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
    }

    // Aktualisiert den Namen einer Gruppe. Gibt null zurück, wenn die Gruppe nicht gefunden wurde.
    public async Task<Group?> UpdateAsync(Group group, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Groups.FindAsync([group.Id], cancellationToken);
        if (existing == null)
        {
            return null;
        }

        existing.Name = group.Name;
        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    // Löscht eine Gruppe; Cascade löscht GroupMembers und setzt GroupId in Expenses auf NULL.
    public async Task<bool> DeleteAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var group = await _context.Groups.FindAsync([groupId], cancellationToken);
        if (group == null)
        {
            return false;
        }

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
