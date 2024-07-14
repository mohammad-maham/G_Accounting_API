using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IUsers
    {
        Task<string> GetSigninAsync(long NationalCode, long Mobile);
        Task<User?> GetSignupAsync(User user);
        Task<bool> IsExistUserAsync(long userId);
    }
}
