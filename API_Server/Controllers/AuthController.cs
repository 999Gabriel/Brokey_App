using API_Server.DTOs;
using API_Server.Extensions;
using API_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ORM;

namespace API_Server.Controllers;

// Controller für Authentifizierung. Route-Präfix: api/auth ([controller] = "Auth").
// Zuständig für Registrierung, Login und das Abrufen des eigenen Profils (/me).
// Dies ist der EINZIGE Controller ohne [Authorize] auf Klassenebene, weil
// Login/Register erreichbar sein müssen, BEVOR der User einen JWT besitzt.
[ApiController]                       // aktiviert automatische Model-Validierung der [Required]/[StringLength]-Attribute der DTOs
[Route("api/[controller]")]           // [controller] wird durch den Klassennamen ohne "Controller" ersetzt → "api/auth"
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;       // direkter EF-Core-Zugang zur MySQL-DB (dieser Controller nutzt kein Repository)
    private readonly TokenService _tokenService;  // erzeugt den signierten JWT nach erfolgreichem Login/Register

    // Empfängt AppDbContext (MySQL-Zugang) und TokenService (JWT-Erstellung) per DI.
    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    // POST /api/auth/register – KEIN [Authorize] (offener Endpunkt). Request-DTO: RegisterRequest.
    // Ablauf: prüft Eindeutigkeit von Email/Username (→ 409 Conflict), hasht das Passwort (BCrypt),
    // speichert den User in der DB und gibt 201 Created mit AuthResponse (inkl. JWT) zurück.
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        // E-Mail wird klein normalisiert, damit Vergleich/Eindeutigkeit case-insensitiv ist; Username nur getrimmt.
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedUsername = request.Username.Trim();

        // Eindeutigkeitsprüfung E-Mail: AnyAsync stellt EINE EXISTS-Query an die DB. Treffer → 409 Conflict.
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
        {
            return Conflict(new { message = "A user with this email already exists." });
        }

        // Eindeutigkeitsprüfung Username (ebenfalls case-insensitiv). Treffer → 409 Conflict.
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername.ToLower()))
        {
            return Conflict(new { message = "This username is already taken." });
        }

        // Neues Domain-User-Objekt aufbauen. WICHTIG: nur der BCrypt-Hash wird gespeichert, niemals das Klartext-Passwort.
        var user = new Models.User
        {
            Username = normalizedUsername,
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),  // BCrypt erzeugt Hash inkl. Salt
            BaseCurrency = request.BaseCurrency.ToUpper().Trim(),             // Währungscode immer in Großbuchstaben (z.B. EUR)
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);                                  // User für das Einfügen vormerken (noch nicht in DB)
        await _context.SaveChangesAsync(HttpContext.RequestAborted);  // INSERT in MySQL ausführen; user.Id wird danach gesetzt

        var response = BuildAuthResponse(user);  // AuthResponse inkl. frisch erzeugtem JWT zusammenbauen

        // 201 Created + Location-Header, der auf GetMe (/api/auth/me) zeigt; Body = AuthResponse.
        return CreatedAtAction(nameof(GetMe), routeValues: null, value: response);
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    // POST /api/auth/login – KEIN [Authorize] (offener Endpunkt). Request-DTO: LoginRequest.
    // Ablauf: sucht User per E-Mail, verifiziert Passwort gegen den gespeicherten BCrypt-Hash,
    // gibt 200 OK + AuthResponse (inkl. JWT) zurück oder 401 Unauthorized bei falscher E-Mail/Passwort.
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        // User per E-Mail laden (SELECT ... LIMIT 1). Existiert keiner → user ist null.
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, HttpContext.RequestAborted);

        // Bewusst identische Fehlermeldung für "User nicht gefunden" und "falsches Passwort",
        // damit ein Angreifer nicht erkennt, ob eine E-Mail registriert ist (Schutz vor User-Enumeration).
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // BCrypt.Verify hasht das eingegebene Passwort mit dem im Hash gespeicherten Salt und vergleicht.
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Erfolg: 200 OK mit AuthResponse (inkl. JWT, den der Client danach im Bearer-Header mitschickt).
        return Ok(BuildAuthResponse(user));
    }

    /// <summary>
    /// Get the currently authenticated user's profile.
    /// </summary>
    // GET /api/auth/me – MIT [Authorize]: nur mit gültigem JWT erreichbar. Kein Request-Body.
    // Ablauf: liest UserId aus dem JWT-Claim (User.GetUserId()), lädt den User aus der DB,
    // gibt 200 OK + UserResponse zurück (404 falls User nicht existiert, 401 ohne gültige Identität).
    // Wird von ProfileViewModel/HomeViewModel aufgerufen, um den eingeloggten User anzuzeigen.
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe(CancellationToken cancellationToken)
    {
        // UserId stammt aus dem JWT (sub/NameIdentifier-Claim), NICHT aus dem Request-Body → kann nicht gefälscht werden.
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();  // 401: Token ungültig oder Claim fehlt
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
        if (user == null)
        {
            return NotFound();  // 404: Token war gültig, aber der User existiert nicht mehr in der DB
        }

        // Bewusst NICHT das User-Domainobjekt zurückgeben (enthält PasswordHash!), sondern ein UserResponse-DTO ohne sensible Felder.
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

    // Baut das AuthResponse-Objekt zusammen: UserId/Username/Email aus dem User-Objekt +
    // frisch generierter JWT aus TokenService → wird an Login und Register zurückgegeben.
    private AuthResponse BuildAuthResponse(Models.User user)
    {
        return new AuthResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            BaseCurrency = user.BaseCurrency,
            Token = _tokenService.GenerateToken(user)
        };
    }
}
