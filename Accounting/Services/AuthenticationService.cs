using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Accounting.Services
{
    public class AuthenticationService : IAuthentication
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly ISMTP _smtp;

        public AuthenticationService(ILogger<AuthenticationService> logger, ISMTP smtp)
        {
            _logger = logger;
            _smtp = smtp;
        }

        public string CreateToken(User user)
        {
            JWTOptions jwtOptions = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings").Get<JWTOptions>()!;
            DateTime expiration = DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinute);
            JwtSecurityToken token = CreateJwtToken(
                CreateClaims(user, jwtOptions),
                CreateSigningCredentials(jwtOptions),
                expiration,
                jwtOptions
            );
            JwtSecurityTokenHandler tokenHandler = new();

            _logger.LogInformation("JWT Token created for UserId: " + user.Id);

            return tokenHandler.WriteToken(token);
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
    }
}
