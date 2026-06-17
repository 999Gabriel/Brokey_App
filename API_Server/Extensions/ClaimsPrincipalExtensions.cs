using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API_Server.Extensions;

public static class ClaimsPrincipalExtensions
{
    // Extension-Methode auf ClaimsPrincipal: liest die UserId (int) aus dem JWT-Sub/NameIdentifier-Claim.
    // Wird in jedem Controller via User.GetUserId() aufgerufen, um den eingeloggten User zu identifizieren.
    // Gibt null zurück, wenn kein passender Claim vorhanden ist.
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)
                    ?? principal.FindFirst("sub");

        return int.TryParse(claim?.Value, out var userId) ? userId : null;
    }
}
