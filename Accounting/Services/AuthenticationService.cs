using Accounting.BusinessLogics;
using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Helpers;
using Accounting.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Accounting.Services
{
    public class AuthenticationService : IAuthentication
    {
        private readonly ISMTP? _smtp;
        private readonly GAccountingDbContext? _accounting;
        private readonly ILogger<AuthenticationService>? _logger;

        public AuthenticationService()
        {
            _accounting = new GAccountingDbContext();
            _smtp = new SMTP();
        }

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
            DateTime expiration = DateTime.Now.AddMinutes(jwtOptions.ExpirationHours);

            // Generate token
            JwtSecurityToken token = CreateJwtToken(
                CreateClaims(user, jwtOptions),
                CreateSigningCredentials(jwtOptions),
                expiration,
                jwtOptions
            );
            JwtSecurityTokenHandler tokenHandler = new();

            _logger!.LogInformation("JWT Token created for UserId: " + user.Id);

            // Stringfy base64 token
            tkn = tokenHandler.WriteToken(token);

            // Log into sessions for first time
            if (!string.IsNullOrEmpty(tkn))
            {
                if (!await _accounting!.SessionMgrs.AnyAsync(x => x.Token == tkn))
                {
                    await _accounting.SessionMgrs.AddAsync(new SessionMgr()
                    {
                        Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_sessionmgr"),
                        Token = tkn,
                        Status = 0,
                        UseDate = DateTime.Now,
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
            string userId = user.Id.ToString();
            string mobile = user.Mobile.ToString()!;
            string nationalCode = user.NationalCode.ToString();

            try
            {
                List<Claim> claims =
                    [
                    new Claim(JwtRegisteredClaimNames.Sub, jwtOptions.JwtRegisteredClaimNamesSub),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.MobilePhone, mobile!),
                    new Claim(ClaimTypes.PrimarySid, nationalCode)
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
            byte[] key = Encoding.UTF8.GetBytes(jwtOptions.SymmetricSecurityKey);
            return new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        }

        public async Task SendOTPAsync(User user, long otp, string origin = "", bool isSMS = true)
        {
            if (!isSMS)
            {
                // Get SMTP Options from appsettings.json
                SMTPOptions smtpOptions = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetSection("SMTPSettings")
                    .Get<SMTPOptions>()!;

                OTPEmail email = new()
                {
                    OTP = otp,
                    Email = user.Email,
                    Subject = origin,
                    Mobile = user.Mobile,
                    NationalCode = user.NationalCode
                };
                // Create Html Body
                StringBuilder mailBody = new();
                mailBody.AppendFormat("<h1>Verification code: </h1>");
                mailBody.AppendFormat("<br />");
                mailBody.AppendFormat("<p>{0}</p>", email.OTP);
                mailBody.AppendFormat("<h5>Gold Marketing</h5>");

                string body = mailBody.ToString();

                SMTPModel smtpModel = new()
                {
                    Options = smtpOptions,
                    To = email.Email,
                    Subject = email.Subject,
                    Body = body
                };

                // Send
                await _smtp!.SendEmailViaGoogleApiAsync(smtpModel);
                //await _smtp!.SendEmailAsync(smtpModel);
            }
            else
            {
                // SMS Configurations
                SMSOptions smsOptions = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetSection("SMSSettings")
                    .Get<SMSOptions>()!;

                SMSOptions sms = new()
                {
                    Host = smsOptions.Host,
                    Username = smsOptions.Username,
                    Password = smsOptions.Password,
                    Source = smsOptions.Source,
                    Message = $"Verification code: #{otp}",
                };
                SMSModel smsModel = new() { Options = sms, Destination = (long)user.Mobile! };

                // Send SMS
                await _smtp!.SendSMSAsync(smsModel);
            }

            // Log OTP into user
            if (user != null)
            {
                string otpInfo = JsonConvert.SerializeObject(new OTPInfo()
                {
                    Origin = origin,
                    OTP = otp,
                    OTPSendDateTime = DateTime.Now
                });
                user.Otpinfo = otpInfo;
                await _accounting!.SaveChangesAsync();
            }
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            long count = await _accounting!.SessionMgrs.CountAsync(x => x.Token == token);
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
                    // Insert into sessions log
                    await _accounting.SessionMgrs.AddAsync(new SessionMgr()
                    {
                        Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_sessionmgr"),
                        Token = token,
                        Status = 0,
                        UseDate = DateTime.Now
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

        public bool IsTokenExpired(DateTime? useDate)
        {
            if (useDate != null)
            {
                DateTime now = DateTime.Now;
                int diff = (int)now.Subtract(useDate.Value).TotalMinutes;
                return diff > 5; // per minute
            }
            return false;
        }

        public string GenerateOTP(int digits)
        {
            Random generator = new();
            string otp = generator.Next(0, 1000000).ToString($"D{digits}");
            return otp;
        }

        public bool VerifyOTPAsync(User user, long otp)
        {
            if (!string.IsNullOrEmpty(user.Otpinfo) && otp != 0)
            {
                OTPInfo otpInfo = JsonConvert.DeserializeObject<OTPInfo>(user.Otpinfo) ?? new OTPInfo();
                DateTime now = DateTime.Now;
                int diff = otpInfo.OTPSendDateTime.Subtract(now).Minutes; // per minute
                if (diff < 5 && otpInfo.OTP == otp)
                {
                    User? validUser = _accounting!.Users.FirstOrDefault(x => x.Id == user.Id);
                    if (validUser != null && validUser.Id != 0)
                    {
                        validUser.Status = 1;
                        _accounting.SaveChanges();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
