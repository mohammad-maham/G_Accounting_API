namespace Accounting.Models
{
    public record class JWTOptions(
        string Issuer,
        string Audience,
        string SymmetricSecurityKey,
        string JwtRegisteredClaimNamesSub,
        int ExpirationMinute
    );
}