using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IAuthentication
    {
        Task<string> CreateTokenAsync(User user);
        Task SendOTPAsync(User user, long otp, string origin = "", bool isSMS = true);
        Task<bool> VerifyTokenAsync(string token);
        bool IsTokenExpired(DateTime? useDate);
        string GenerateOTP(int digits);
        bool VerifyOTPAsync(User user, long otp);
    }
}
