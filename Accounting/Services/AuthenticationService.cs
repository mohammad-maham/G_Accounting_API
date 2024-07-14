using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Accounting.Services
{
    public class AuthenticationService : IAuthentication
    {
        private readonly ISMTP _smtp;
        private readonly GAccountingDbContext _accounting;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(ILogger<AuthenticationService> logger, ISMTP smtp, GAccountingDbContext accounting)
        {
            _logger = logger;
            _smtp = smtp;
            _accounting = accounting;
        }

        public async Task<string> CreateTokenAsync(User user)
        {
            string tkn = string.Empty;
            JWTOptions jwtOptions = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings").Get<JWTOptions>()!;
            DateTime expiration = DateTime.UtcNow.AddHours(jwtOptions.ExpirationHours);
            JwtSecurityToken token = CreateJwtToken(
                CreateClaims(user, jwtOptions),
                CreateSigningCredentials(jwtOptions),
                expiration,
                jwtOptions
            );
            JwtSecurityTokenHandler tokenHandler = new();

            _logger.LogInformation("JWT Token created for UserId: " + user.Id);

            tkn = tokenHandler.WriteToken(token);
            if (!string.IsNullOrEmpty(tkn))
            {
                if (!await _accounting.SessionMgrs.AnyAsync(x => x.Token == tkn))
                {
                    await _accounting.SessionMgrs.AddAsync(new SessionMgr()
                    {
                        Token = tkn,
                        Status = 0,
                        UseDate = DateTimeOffset.Now
                    });
                    await _accounting.SaveChangesAsync();
                }
            }
            return tkn;
        }

        private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
            DateTime expiration, JWTOptions jwtOptions)
        {
            return new(jwtOptions.Issuer,
                jwtOptions.Audience,
                claims,
                expires: expiration,
                signingCredentials: credentials
            );
        }

        private List<Claim> CreateClaims(User user, JWTOptions jwtOptions)
        {
            try
            {
                List<Claim> claims =
                [
                new Claim(JwtRegisteredClaimNames.Sub, jwtOptions.JwtRegisteredClaimNamesSub),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.MobilePhone, user.Mobile.ToString()),
                new Claim(ClaimTypes.PrimarySid, user.NationalCode.ToString())
            ];

                return claims;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private SigningCredentials CreateSigningCredentials(JWTOptions jwtOptions)
        {
            return new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.SymmetricSecurityKey)
                ),
                SecurityAlgorithms.HmacSha256
            );
        }

        public async Task SendOTPEmailAsync(OTPEmail email)
        {
            // Get SMTP Options from appsettings.json
            SMTPOptions smtpOptions = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("SmtpSettings")
                .Get<SMTPOptions>()!;

            // Create Html Body
            StringBuilder mailBody = new();
            mailBody.AppendFormat("<h1>Verification code: </h1>");
            mailBody.AppendFormat("<br />");
            mailBody.AppendFormat("<p>{0}</p>", email.OTP);

            string body = mailBody.ToString();

            SMTPModel smtpModel = new()
            {
                Options = smtpOptions,
                To = email.To,
                Subject = email.Subject,
                Body = body
            };
            await _smtp.SendEmailAsync(smtpModel);
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            long count = await _accounting.SessionMgrs.CountAsync(x => x.Token == token);
            if (count > 0)
            {
                List<SessionMgr> sessions = await _accounting.SessionMgrs.Where(x => x.Token == token).OrderBy(x => x.UseDate).ToListAsync();
                SessionMgr session = sessions.Last();
                if (IsTokenExpired(session.UseDate))
                {
                    return false;
                }
                else
                {
                    await _accounting.SessionMgrs.AddAsync(new SessionMgr()
                    {
                        Id = session.Id,
                        Token = token,
                        Status = 0,
                        UseDate = DateTimeOffset.Now
                    });
                    await _accounting.SaveChangesAsync();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsTokenExpired(DateTimeOffset? useDate)
        {
            if (useDate != null)
            {
                TimeSpan diff = useDate?.Subtract(DateTimeOffset.Now) ?? new TimeSpan();
                return diff.TotalMinutes > 5; // per minute
            }
            return false;
        }
    }
}
