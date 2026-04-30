using API_Server.DTOs;
using API_Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using ORM.Repositories;

namespace API_Server.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly TripRepository _tripRepository;
    private readonly GroupRepository _groupRepository;
    private readonly GroupMemberRepository _groupMemberRepository;
    private readonly TripMemberRepository _tripMemberRepository;
    private readonly ExpenseRepository _expenseRepository;

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

    [HttpPost]
    public async Task<ActionResult<TripDetailResponse>> CreateTrip(
        [FromBody] CreateTripRequest request,
        CancellationToken cancellationToken)
    {
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

        await _tripRepository.CreateAsync(trip, cancellationToken);
        await _tripMemberRepository.AddParticipantAsync(trip.Id, userId.Value, "Owner", cancellationToken);

        var createdTrip = await _tripRepository.GetByIdForUserAsync(trip.Id, userId.Value, cancellationToken);
        return CreatedAtAction(nameof(GetTripById), new { id = trip.Id }, MapTripDetail(createdTrip!));
    }

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
