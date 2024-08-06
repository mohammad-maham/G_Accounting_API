using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IAuthentication
    {
        string CreateToken(User user);
        void SendOTP(User user, long otp, string origin = "", bool isSMS = true);
        bool VerifyToken(string token, bool isLogToSession = true);
        bool IsTokenExpired(DateTime? useDate);
        string GenerateOTP(int digits);
        bool VerifyOTP(User user, long otp);
    }
}
