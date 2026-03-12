using Microsoft.EntityFrameworkCore;
using Models;

namespace ORM.Repositories;

public class TripMemberRepository
{
    private readonly AppDbContext _context;

    public TripMemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TripMember?> AddParticipantAsync(
        int tripId,
        int userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.TripMembers
            .Include(tm => tm.User)
            .FirstOrDefaultAsync(
                tm => tm.TripId == tripId && tm.UserId == userId,
                cancellationToken);

        if (existing != null)
        {
            if (!string.Equals(existing.Role, "Owner", StringComparison.OrdinalIgnoreCase))
            {
                existing.Role = role;
            }
            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var member = new TripMember
        {
            TripId = tripId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        _context.TripMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        return await _context.TripMembers
            .Include(tm => tm.User)
            .FirstAsync(tm => tm.Id == member.Id, cancellationToken);
    }

    public async Task<bool> RemoveParticipantAsync(int tripId, int userId, CancellationToken cancellationToken = default)
    {
        var member = await _context.TripMembers
            .FirstOrDefaultAsync(
                tm => tm.TripId == tripId && tm.UserId == userId,
                cancellationToken);

        if (member == null)
        {
            return false;
        }

        _context.TripMembers.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<TripMember>> GetMembersByTripAsync(int tripId, CancellationToken cancellationToken = default)
    {
        return await _context.TripMembers
            .AsNoTracking()
            .Include(tm => tm.User)
            .Where(tm => tm.TripId == tripId)
            .OrderByDescending(tm => tm.Role)
            .ThenBy(tm => tm.User.Username)
            .ToListAsync(cancellationToken);
    }
}
