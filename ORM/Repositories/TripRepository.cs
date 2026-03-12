using Microsoft.EntityFrameworkCore;
using Models;

namespace ORM.Repositories;

public class TripRepository
{
    private readonly AppDbContext _context;

    public TripRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Trip> CreateAsync(Trip trip, CancellationToken cancellationToken = default)
    {
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync(cancellationToken);
        return trip;
    }

    public async Task<List<Trip>> GetTripsByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .AsNoTracking()
            .Include(t => t.Groups)
            .Include(t => t.Members)
            .Where(t => t.CreatedById == userId || t.Members.Any(m => m.UserId == userId))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Trip?> GetByIdForUserAsync(int tripId, int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .Include(t => t.Groups)
                .ThenInclude(g => g.Members)
                    .ThenInclude(gm => gm.User)
            .Include(t => t.Members)
            .FirstOrDefaultAsync(
                t => t.Id == tripId && (t.CreatedById == userId || t.Members.Any(m => m.UserId == userId)),
                cancellationToken);
    }

    public async Task<Trip?> GetByIdAsync(int tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .Include(t => t.Groups)
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == tripId, cancellationToken);
    }

    public async Task<Trip?> UpdateAsync(Trip trip, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Trips.FindAsync([trip.Id], cancellationToken);
        if (existing == null)
        {
            return null;
        }

        existing.Name = trip.Name;
        existing.Description = trip.Description;
        existing.BaseCurrency = trip.BaseCurrency;
        existing.StartDate = trip.StartDate;
        existing.EndDate = trip.EndDate;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(int tripId, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips.FindAsync([tripId], cancellationToken);
        if (trip == null)
        {
            return false;
        }

        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
