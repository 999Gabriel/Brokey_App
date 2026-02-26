using API_Server.DTOs;
using API_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ORM;

namespace API_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return Conflict(new { message = "A user with this email already exists." });
        }

        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return Conflict(new { message = "This username is already taken." });
        }

        // Create user
        var user = new Models.User
        {
            Username = request.Username,
            Email = request.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            BaseCurrency = request.BaseCurrency.ToUpper().Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = _tokenService.GenerateToken(user);

        return CreatedAtAction(nameof(GetMe), new AuthResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            BaseCurrency = user.BaseCurrency,
            Token = token
        });
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            BaseCurrency = user.BaseCurrency,
            Token = token
        });
    }

    /// <summary>
    /// Get the currently authenticated user's profile.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                          ?? User.FindFirst("sub");

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            BaseCurrency = user.BaseCurrency,
            ProfileImagePath = user.ProfileImagePath,
            CreatedAt = user.CreatedAt
        });
    }
}

