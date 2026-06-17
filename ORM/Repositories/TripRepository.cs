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

    // Fügt einen neuen Trip in die DB ein und gibt das gespeicherte Objekt zurück.
    // → TripsController.CreateTrip ruft dies auf, danach folgt AddParticipantAsync.
    public async Task<Trip> CreateAsync(Trip trip, CancellationToken cancellationToken = default)
    {
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync(cancellationToken);
        return trip;
    }

    // Lädt alle Trips des Users (als Ersteller oder TripMember) inkl. Groups, Expenses und Members.
    // Sortiert nach Erstellungsdatum absteigend → TripsController.GetTrips → TripsViewModel.
    public async Task<List<Trip>> GetTripsByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .AsNoTracking()
            .Include(t => t.Groups)
                .ThenInclude(g => g.Expenses)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Expenses)
            .Where(t => t.CreatedById == userId || t.Members.Any(m => m.UserId == userId))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // Lädt einen Trip mit vollständigen Navigation-Properties NUR wenn der User Mitglied ist.
    // Sicherheitscheck: verhindert, dass fremde Trips über die ID aufgerufen werden können.
    public async Task<Trip?> GetByIdForUserAsync(int tripId, int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .Include(t => t.CreatedBy)
            .Include(t => t.Groups)
                .ThenInclude(g => g.Members)
                    .ThenInclude(gm => gm.User)
            .Include(t => t.Groups)
                .ThenInclude(g => g.Expenses)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Expenses)
            .FirstOrDefaultAsync(
                t => t.Id == tripId && (t.CreatedById == userId || t.Members.Any(m => m.UserId == userId)),
                cancellationToken);
    }

    // Lädt einen Trip ohne User-Filter (intern verwendet, z.B. für Admin-Operationen).
    public async Task<Trip?> GetByIdAsync(int tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .Include(t => t.CreatedBy)
            .Include(t => t.Groups)
                .ThenInclude(g => g.Expenses)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Expenses)
            .FirstOrDefaultAsync(t => t.Id == tripId, cancellationToken);
    }

    // Aktualisiert Name, Beschreibung, Währung und Daten eines bestehenden Trips. Gibt null zurück, wenn nicht gefunden.
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

    // Löscht einen Trip; durch Cascade in der DB werden auch Groups, Expenses und TripMembers gelöscht.
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
