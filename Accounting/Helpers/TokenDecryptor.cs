using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Accounting.Helpers
{
    public class TokenDecryptor
    {
        public static long GetUserIdByToken(string token)
        {
            JwtSecurityTokenHandler? handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtSecurityToken = handler.ReadJwtToken(token);
            string? userIdClaim = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value ?? "0";
            long userId = long.TryParse(userIdClaim, out long user_id) ? user_id : 0;
            return userId;
        }
    }
}
