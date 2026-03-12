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

    public TripsController(
        TripRepository tripRepository,
        GroupRepository groupRepository,
        GroupMemberRepository groupMemberRepository,
        TripMemberRepository tripMemberRepository)
    {
        _tripRepository = tripRepository;
        _groupRepository = groupRepository;
        _groupMemberRepository = groupMemberRepository;
        _tripMemberRepository = tripMemberRepository;
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
        return new TripDetailResponse
        {
            Id = trip.Id,
            Name = trip.Name,
            Description = trip.Description,
            BaseCurrency = trip.BaseCurrency,
            CreatedById = trip.CreatedById,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            CreatedAt = trip.CreatedAt,
            Groups = trip.Groups
                .OrderBy(g => g.Name)
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
            MemberCount = group.Members.Count
        };
    }
}
