using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IDashboard
    {
        Task<UserInfo> GetUserInfoAsync(long userId);
    }
}
