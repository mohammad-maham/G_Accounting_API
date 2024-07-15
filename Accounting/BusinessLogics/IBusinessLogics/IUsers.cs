using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IUsers
    {
        Task<string> GetSigninAsync(string username, string password);
        Task<User?> GetSignupAsync(User user);
        Task<bool> IsExistUserAsync(long userId);
        Task<User?> FindUserInfoAsync(string username);
        Task<User?> FindUserInfoAsync(string username, string password);
        Task SetPasswordAsync(string username, string password);
    }
}
