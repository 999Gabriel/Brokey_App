using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API_Server.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)
                    ?? principal.FindFirst("sub");

        return int.TryParse(claim?.Value, out var userId) ? userId : null;
    }
}
