using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IDashboard
    {
        DashboardVM GetUserInfo(long userId);
    }
}
