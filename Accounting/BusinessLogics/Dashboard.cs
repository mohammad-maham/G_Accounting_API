using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;

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

        public DashboardVM GetUserInfo(long userId)
        {
            UserInfo? userInfo = new();
            List<MenusVM>? menus = null;
            DashboardVM dashboard = new();
            if (userId != 0)
            {
                userInfo = _accounting.UserInfos.FirstOrDefault(ui => ui.UserId == userId);

                menus = _accounting.Users
                    .SelectMany(usr => _accounting.UserRoles.Where(ur => ur.UserId == usr.Id).DefaultIfEmpty(), (users, userRoles) => new { users, userRoles })
                    .SelectMany(ur => _accounting.Roles.Where(r => r.Id == ur.userRoles!.RoleId).DefaultIfEmpty(), (ur, roles) => new { ur, roles })
                    .SelectMany(ro => _accounting.RoleAccesses.Where(ra => ra.RoleId == ro.roles!.Id).DefaultIfEmpty(), (ro, roleAccess) => new { ro, roleAccess })
                    .SelectMany(roa => _accounting.Menus.Where(m => m.Id == roa.roleAccess!.MenuId), (roa, menu) => new { roa, menu })
                    .SelectMany(ra => _accounting.Actions.Where(a => a.Id == ra.roa.roleAccess!.ActionId), (ra, ac) => new { ra, ac })
                    .Where(w => w.ac.Status == 1 && w.ra.roa.ro.ur.users.Id == userId)
                    .Select(x => new MenusVM()
                    {
                        ActionName = x.ac.Name,
                        ActionPath = x.ac.Path,
                        MenuName = x.ra.menu.Name,
                        MenuTitle = x.ra.menu.Title
                    }).ToList();

                UserRoleVM? roles = _accounting.Users
                    .SelectMany(usr => _accounting.UserRoles.Where(ur => ur.UserId == usr.Id), (usr, ur) => new { usr, ur })
                    .SelectMany(urs => _accounting.Roles.Where(r => r.Id == urs.ur.RoleId), (urs, r) => new { urs, r })
                    .Where(w => w.urs.usr.Id == userId)
                    .Select(x => new UserRoleVM() { Name = x.r.Name, Title = x.r.Description }).FirstOrDefault();

                dashboard.UserInfo = userInfo;
                dashboard.Menus = menus;
                dashboard.UserRole = roles;
            }
            return dashboard;
        }
    }
}
