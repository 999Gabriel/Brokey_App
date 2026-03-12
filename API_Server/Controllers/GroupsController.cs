using API_Server.DTOs;
using API_Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ORM;
using ORM.Repositories;

namespace API_Server.Controllers;

[ApiController]
[Authorize]
[Route("api/groups")]
public class GroupsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly GroupRepository _groupRepository;
    private readonly GroupMemberRepository _groupMemberRepository;
    private readonly TripRepository _tripRepository;
    private readonly TripMemberRepository _tripMemberRepository;

    public GroupsController(
        AppDbContext context,
        GroupRepository groupRepository,
        GroupMemberRepository groupMemberRepository,
        TripRepository tripRepository,
        TripMemberRepository tripMemberRepository)
    {
        _context = context;
        _groupRepository = groupRepository;
        _groupMemberRepository = groupMemberRepository;
        _tripRepository = tripRepository;
        _tripMemberRepository = tripMemberRepository;
    }

    [HttpPost("{id:int}/members")]
    public async Task<ActionResult<GroupMemberResponse>> AddMember(
        int id,
        [FromBody] AddGroupMemberRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);

        if (group == null)
        {
            return NotFound(new { message = "Group not found." });
        }

        var trip = await _tripRepository.GetByIdForUserAsync(group.TripId, currentUserId.Value, cancellationToken);
        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        var normalizedIdentifier = request.UsernameOrEmail.Trim().ToLowerInvariant();
        var targetUser = await _context.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == normalizedIdentifier || u.Username.ToLower() == normalizedIdentifier,
            cancellationToken);

        if (targetUser == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var normalizedRole = NormalizeGroupRole(request.Role);
        if (normalizedRole == null)
        {
            return BadRequest(new { message = "Role must be Admin or Member." });
        }

        await _tripMemberRepository.AddParticipantAsync(group.TripId, targetUser.Id, "Member", cancellationToken);
        var member = await _groupMemberRepository.AddMemberAsync(id, targetUser.Id, normalizedRole, cancellationToken);
        return Ok(MapMember(member!));
    }

    [HttpDelete("{id:int}/members/{userId:int}")]
    public async Task<IActionResult> RemoveMember(int id, int userId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);

        if (group == null)
        {
            return NotFound(new { message = "Group not found." });
        }

        var trip = await _tripRepository.GetByIdForUserAsync(group.TripId, currentUserId.Value, cancellationToken);
        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        var removed = await _groupMemberRepository.RemoveMemberAsync(id, userId, cancellationToken);
        if (!removed)
        {
            return NotFound(new { message = "Member not found in this group." });
        }

        return NoContent();
    }

    [HttpGet("{id:int}/members")]
    public async Task<ActionResult<List<GroupMemberResponse>>> GetMembers(int id, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);

        if (group == null)
        {
            return NotFound(new { message = "Group not found." });
        }

        var trip = await _tripRepository.GetByIdForUserAsync(group.TripId, currentUserId.Value, cancellationToken);
        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(id, cancellationToken);
        return Ok(members.Select(MapMember).ToList());
    }

    private static GroupMemberResponse MapMember(Models.GroupMember member)
    {
        return new GroupMemberResponse
        {
            UserId = member.UserId,
            Username = member.User.Username,
            Email = member.User.Email,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        };
    }

    private static string? NormalizeGroupRole(string role)
    {
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Admin";
        }

        if (string.Equals(role, "Member", StringComparison.OrdinalIgnoreCase))
        {
            return "Member";
        }

        return null;
    }
}
