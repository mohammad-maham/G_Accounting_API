using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Accounting.Helpers
{
    public class TokenDecryptor
    {
        public static long GetUserIdByToken(string token)
        {
            long userId = 0;
            SecurityToken validatedToken;
            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            byte[] Key = Encoding.UTF8.GetBytes(config["JwtTokenSettings:SymmetricSecurityKey"]!);
            var validationParameters = new TokenValidationParameters()
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["JwtTokenSettings:Issuer"],
                ValidAudience = config["JwtTokenSettings:Audience"],
                LifetimeValidator = TokenLifetimeValidator.Validate,
                IssuerSigningKey = new SymmetricSecurityKey(Key)
            };
            JwtSecurityTokenHandler? handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtSecurityToken = handler.ReadJwtToken(token);
            string? userIdClaim = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value ?? "0";
            bool isAuthenticated = handler.ValidateToken(token, validationParameters, out validatedToken).Identity?.IsAuthenticated ?? false;
            if (isAuthenticated)
                userId = long.TryParse(userIdClaim, out long user_id) ? user_id : 0;
            return userId;
        }
    }
}
