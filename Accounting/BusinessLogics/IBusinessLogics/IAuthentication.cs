using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IAuthentication
    {
        Task<string> CreateTokenAsync(User user);
        Task SendOTPEmailAsync(OTPEmail email);
        Task<bool> VerifyTokenAsync(string token);
        bool IsTokenExpired(DateTimeOffset? useDate);
        string GenerateOTP(int digits);
    }
}
