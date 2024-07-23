using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IDashboard
    {
        Task<DashboardVM> GetUserInfoAsync(long userId);
    }
}
