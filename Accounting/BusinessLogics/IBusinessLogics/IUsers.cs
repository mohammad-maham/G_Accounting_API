using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IUsers
    {
        Task<string> GetSignin(long NationalCode, long Mobile);
        Task<User?> GetSignup(User user);
        Task<bool> IsExistUser(long userId);
    }
}
