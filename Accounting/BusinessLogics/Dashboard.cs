using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;
using Microsoft.EntityFrameworkCore;

namespace Accounting.BusinessLogics
{
    public class Dashboard : IDashboard
    {
        private readonly ILogger<Dashboard> _logger;
        private readonly GAccountingDbContext _accounting;

        public Dashboard(ILogger<Dashboard> logger, GAccountingDbContext accounting)
        {
            _logger = logger;
            _accounting = accounting;
        }

        public async Task<UserInfo> GetUserInfoAsync(long userId)
        {
            UserInfo? userInfo = new();
            if (userId != 0)
            {
                userInfo = await _accounting.UserInfos.FirstOrDefaultAsync(x => x.UserId == userId);
            }
            return userInfo;
        }
    }
}
