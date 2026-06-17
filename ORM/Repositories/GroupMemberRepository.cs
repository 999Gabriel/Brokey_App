using Microsoft.EntityFrameworkCore;
using Models;

namespace ORM.Repositories;

public class GroupMemberRepository
{
    private readonly AppDbContext _context;

    public GroupMemberRepository(AppDbContext context)
    {
        _context = context;
    }

    // Fügt einen User als GroupMember hinzu. Existiert er bereits, wird nur die Rolle aktualisiert (Upsert).
    // Lädt den Member nach dem Speichern mit User-Navigation zurück.
    // → GroupsController.AddMember und TripsController.CreateGroup nutzen dies.
    public async Task<GroupMember?> AddMemberAsync(
        int groupId,
        int userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.GroupMembers
            .Include(gm => gm.User)
            .FirstOrDefaultAsync(
                gm => gm.GroupId == groupId && gm.UserId == userId,
                cancellationToken);

        if (existing != null)
        {
            existing.Role = role;
            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var member = new GroupMember
        {
            GroupId = groupId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        _context.GroupMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        return await _context.GroupMembers
            .Include(gm => gm.User)
            .FirstAsync(gm => gm.Id == member.Id, cancellationToken);
    }

    // Entfernt einen User aus der Gruppe. Gibt false zurück, wenn kein solcher Member gefunden wurde.
    public async Task<bool> RemoveMemberAsync(int groupId, int userId, CancellationToken cancellationToken = default)
    {
        var member = await _context.GroupMembers
            .FirstOrDefaultAsync(
                gm => gm.GroupId == groupId && gm.UserId == userId,
                cancellationToken);

        if (member == null)
        {
            return false;
        }

        _context.GroupMembers.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Gibt alle Mitglieder einer Gruppe zurück: Admin zuerst, danach alphabetisch nach Username.
    public async Task<List<GroupMember>> GetMembersByGroupAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .AsNoTracking()
            .Include(gm => gm.User)
            .Where(gm => gm.GroupId == groupId)
            .OrderByDescending(gm => gm.Role)
            .ThenBy(gm => gm.User.Username)
            .ToListAsync(cancellationToken);
    }
}
