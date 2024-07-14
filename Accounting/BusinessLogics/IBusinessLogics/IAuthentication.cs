using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IAuthentication
    {
        string CreateToken(User user);
        Task SendOTPEmailAsync(OTPEmail email);
        Task<bool> VerifyTokenAsync(string token);
        bool IsTokenExpired(string token);
    }
}
