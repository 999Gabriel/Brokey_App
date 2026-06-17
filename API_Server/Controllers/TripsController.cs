using API_Server.DTOs;
using API_Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using ORM.Repositories;

namespace API_Server.Controllers;

// Controller für Reisen (Trips). Route-Präfix: api/trips ([controller] = "Trips").
// Zuständig für: Trips erstellen/auflisten/Details laden, Gruppen innerhalb eines Trips anlegen/auflisten
// und die letzten Aktivitäten (recent-activities) für die Startseite liefern.
// [Authorize] auf Klassenebene → JEDE Action verlangt einen gültigen JWT.
[ApiController]      // automatische DTO-Validierung + automatische 400-Antworten bei ungültigem Request-Body
[Authorize]          // gilt für alle Actions: ohne gültigen Bearer-Token → 401
[Route("api/[controller]")]  // [controller] → Klassenname ohne "Controller" → "api/trips"
public class TripsController : ControllerBase
{
    private readonly TripRepository _tripRepository;             // Trips laden/erstellen + Mitgliedschaft prüfen
    private readonly GroupRepository _groupRepository;           // Gruppen laden/erstellen
    private readonly GroupMemberRepository _groupMemberRepository;  // Gruppenmitglieder (Ersteller wird Admin)
    private readonly TripMemberRepository _tripMemberRepository;    // Trip-Teilnehmer (Ersteller wird Owner)
    private readonly ExpenseRepository _expenseRepository;       // Ausgaben für recent-activities

    // Alle Repositories werden per DI injiziert; jedes Repository kapselt die DB-Queries für eine Entität.
    public TripsController(
        TripRepository tripRepository,
        GroupRepository groupRepository,
        GroupMemberRepository groupMemberRepository,
        TripMemberRepository tripMemberRepository,
        ExpenseRepository expenseRepository)
    {
        _tripRepository = tripRepository;
        _groupRepository = groupRepository;
        _groupMemberRepository = groupMemberRepository;
        _tripMemberRepository = tripMemberRepository;
        _expenseRepository = expenseRepository;
    }

    // GET /api/trips/recent-activities – liefert die 10 neuesten Ausgaben aller Trips, an denen der User beteiligt ist.
    // Daten: ExpenseRepository → MapExpense → List<ExpenseResponse> → HomeViewModel (RecentActivities).
    [HttpGet("recent-activities")]
    public async Task<ActionResult<List<ExpenseResponse>>> GetRecentActivities(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var expenses = await _expenseRepository.GetRecentExpensesForUserAsync(userId.Value, 10, cancellationToken);
        return Ok(expenses.Select(MapExpense).ToList());
    }

    // POST /api/trips – [Authorize]. Request-DTO: CreateTripRequest. Response: TripDetailResponse, 201 Created.
    // Erstellt einen neuen Trip und fügt den Ersteller automatisch als "Owner"-TripMember hinzu.
    // Statuscodes: 201 (erstellt), 400 (EndDate vor StartDate), 401 (kein Token).
    // Daten: CreateTripRequest → Trip+TripMember in DB → TripDetailResponse → CreateTripViewModel.
    [HttpPost]
    public async Task<ActionResult<TripDetailResponse>> CreateTrip(
        [FromBody] CreateTripRequest request,
        CancellationToken cancellationToken)
    {
        // Plausibilitätsprüfung des Zeitraums: Enddatum darf nicht vor dem Startdatum liegen → 400.
        if (request.EndDate.Date < request.StartDate.Date)
        {
            return BadRequest(new { message = "End date must be on or after the start date." });
        }

        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var trip = new Trip
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            BaseCurrency = request.BaseCurrency.Trim().ToUpperInvariant(),
            CreatedById = userId.Value,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            CreatedAt = DateTime.UtcNow
        };

        await _tripRepository.CreateAsync(trip, cancellationToken);  // INSERT; setzt trip.Id
        // Ersteller wird sofort als "Owner" eingetragen, sonst hätte er keinen Zugriff auf seinen eigenen Trip.
        await _tripMemberRepository.AddParticipantAsync(trip.Id, userId.Value, "Owner", cancellationToken);

        // Trip frisch mit allen Navigation-Properties (Members, Groups, ...) neu laden, damit das DTO vollständig ist.
        var createdTrip = await _tripRepository.GetByIdForUserAsync(trip.Id, userId.Value, cancellationToken);
        // 201 Created + Location-Header auf GET /api/trips/{id}.
        return CreatedAtAction(nameof(GetTripById), new { id = trip.Id }, MapTripDetail(createdTrip!));
    }

    // GET /api/trips – liefert alle Trips des eingeloggten Users (als Ersteller oder TripMember).
    // Daten: TripRepository → MapTripSummary → List<TripSummaryResponse> → TripsViewModel.
    [HttpGet]
    public async Task<ActionResult<List<TripSummaryResponse>>> GetTrips(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var trips = await _tripRepository.GetTripsByUserAsync(userId.Value, cancellationToken);
        return Ok(trips.Select(MapTripSummary).ToList());
    }

    // GET /api/trips/{id} – lädt einen einzelnen Trip mit allen Groups, Members und Expenses.
    // Prüft dabei, dass der eingeloggte User Mitglied des Trips ist (Sicherheitscheck im Repository).
    // → TripDetailViewModel und TripSummaryViewModel lesen diese Daten.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TripDetailResponse>> GetTripById(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var trip = await _tripRepository.GetByIdForUserAsync(id, userId.Value, cancellationToken);

        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        return Ok(MapTripDetail(trip));
    }

    // POST /api/trips/{id}/groups – erstellt eine neue Gruppe im Trip, fügt den Ersteller als "Admin" ein.
    // Daten: CreateGroupRequest → Group+GroupMember in DB → GroupResponse → TripDetailViewModel.
    [HttpPost("{id:int}/groups")]
    public async Task<ActionResult<GroupResponse>> CreateGroup(
        int id,
        [FromBody] CreateGroupRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var trip = await _tripRepository.GetByIdForUserAsync(id, userId.Value, cancellationToken);

        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        var group = new Group
        {
            Name = request.Name.Trim(),
            TripId = id,
            CreatedById = userId.Value,
            CreatedAt = DateTime.UtcNow
        };

        await _groupRepository.CreateAsync(group, cancellationToken);
        await _groupMemberRepository.AddMemberAsync(group.Id, userId.Value, "Admin", cancellationToken);

        var createdGroup = await _groupRepository.GetByIdAsync(group.Id, cancellationToken);
        return CreatedAtAction(nameof(GetTripGroups), new { id }, MapGroup(createdGroup!));
    }

    // GET /api/trips/{id}/groups – gibt alle Gruppen eines Trips zurück inkl. Member- und Expense-Anzahl.
    // → TripService.GetGroupsAsync → TripDetailViewModel.Groups.
    [HttpGet("{id:int}/groups")]
    public async Task<ActionResult<List<GroupResponse>>> GetTripGroups(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var trip = await _tripRepository.GetByIdForUserAsync(id, userId.Value, cancellationToken);

        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        var groups = await _groupRepository.GetByTripIdAsync(id, cancellationToken);
        return Ok(groups.Select(MapGroup).ToList());
    }

    // Wandelt ein Expense-Domainobjekt (mit allen Navigation-Properties) in ein flaches ExpenseResponse-DTO um.
    // Berechnet OwesAmount pro Split (0 wenn der Split-User auch der Zahler ist).
    private static ExpenseResponse MapExpense(Expense expense)
    {
        return new ExpenseResponse
        {
            Id = expense.Id,
            TripId = expense.TripId,
            GroupId = expense.GroupId ?? 0,
            GroupName = expense.Group?.Name ?? string.Empty,
            Currency = expense.Group?.Trip.BaseCurrency ?? expense.Trip.BaseCurrency,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category.Name,
            PaidByUserId = expense.PaidByUserId,
            PaidByUsername = expense.PaidBy.Username,
            Title = expense.Title,
            Description = expense.Description,
            Amount = expense.Amount,
            Latitude = expense.Latitude,
            Longitude = expense.Longitude,
            ExpenseDate = expense.ExpenseDate,
            CreatedAt = expense.CreatedAt,
            Splits = expense.Splits
                .OrderBy(s => s.User.Username)
                .Select(s => new ExpenseSplitResponse
                {
                    UserId = s.UserId,
                    Username = s.User.Username,
                    Amount = s.Amount,
                    PaidToUserId = expense.PaidByUserId,
                    PaidToUsername = expense.PaidBy.Username,
                    OwesAmount = s.UserId == expense.PaidByUserId ? 0 : s.Amount,
                    IsPaidByUser = s.UserId == expense.PaidByUserId,
                    IsSettled = s.IsSettled,
                    SettledAt = s.SettledAt
                })
                .ToList()
        };
    }

    // Flacht einen Trip auf eine kompakte Übersicht (Name, Datum, Anzahlen) für die Trips-Liste ab.
    // → TripsViewModel zeigt diese Daten in der Kachel-Liste an.
    private static TripSummaryResponse MapTripSummary(Trip trip)
    {
        return new TripSummaryResponse
        {
            Id = trip.Id,
            Name = trip.Name,
            Description = trip.Description,
            BaseCurrency = trip.BaseCurrency,
            CreatedById = trip.CreatedById,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            CreatedAt = trip.CreatedAt,
            GroupCount = trip.Groups.Count,
            MemberCount = trip.Members.Count
        };
    }

    // Wandelt einen Trip in das vollständige Detail-DTO um: sortierte Members (Owner zuerst),
    // sortierte Groups (alphabetisch), berechnete Gesamtausgaben und Reisedauer in Tagen.
    // → TripDetailViewModel und TripSummaryViewModel konsumieren dieses DTO.
    private static TripDetailResponse MapTripDetail(Trip trip)
    {
        var orderedGroups = trip.Groups
            .OrderBy(g => g.Name)
            .ToList();

        var orderedMembers = trip.Members
            .OrderByDescending(member => string.Equals(member.Role, "Owner", StringComparison.OrdinalIgnoreCase))
            .ThenBy(member => member.User.Username)
            .ToList();

        return new TripDetailResponse
        {
            Id = trip.Id,
            Name = trip.Name,
            Description = trip.Description,
            BaseCurrency = trip.BaseCurrency,
            CreatedById = trip.CreatedById,
            CreatedByUsername = trip.CreatedBy.Username,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            CreatedAt = trip.CreatedAt,
            // Reisedauer inklusive Start- UND Endtag (+1), mindestens 1 Tag (Math.Max verhindert 0 bei eintägigem Trip).
            DurationDays = Math.Max(1, (trip.EndDate.Date - trip.StartDate.Date).Days + 1),
            GroupCount = orderedGroups.Count,
            MemberCount = orderedMembers.Count,
            ExpenseCount = trip.Expenses.Count,
            TotalExpenseAmount = decimal.Round(trip.Expenses.Sum(expense => expense.Amount), 2),
            Members = orderedMembers
                .Select(member => new TripMemberResponse
                {
                    UserId = member.UserId,
                    Username = member.User.Username,
                    Email = member.User.Email,
                    Role = member.Role,
                    JoinedAt = member.JoinedAt
                })
                .ToList(),
            Groups = orderedGroups
                .Select(MapGroup)
                .ToList()
        };
    }

    // Flacht ein Group-Objekt auf ein DTO ab: zählt Members und Expenses, summiert den Gesamtbetrag.
    // → Wird in TripDetailResponse.Groups und GetTripGroups verwendet.
    private static GroupResponse MapGroup(Group group)
    {
        return new GroupResponse
        {
            Id = group.Id,
            TripId = group.TripId,
            Name = group.Name,
            CreatedById = group.CreatedById,
            CreatedAt = group.CreatedAt,
            MemberCount = group.Members.Count,
            ExpenseCount = group.Expenses.Count,
            TotalExpenseAmount = decimal.Round(group.Expenses.Sum(expense => expense.Amount), 2)
        };
    }
}
