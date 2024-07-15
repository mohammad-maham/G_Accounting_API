using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IAuthentication
    {
        Task<string> CreateTokenAsync(User user);
        Task SendOTPEmailAsync(OTPEmail email);
        Task<bool> VerifyTokenAsync(string token);
        bool IsTokenExpired(DateTime? useDate);
        string GenerateOTP(int digits);
        bool VerifyOTPAsync(User user, long otp);
    }
}
