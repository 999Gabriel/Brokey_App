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

    public async Task<Group> CreateAsync(Group group, CancellationToken cancellationToken = default)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
        return group;
    }

    public async Task<List<Group>> GetByTripIdAsync(int tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .AsNoTracking()
            .Include(g => g.Members)
            .Where(g => g.TripId == tripId)
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Group?> GetByIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.Trip)
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
    }

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
