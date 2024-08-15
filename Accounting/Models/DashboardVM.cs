namespace Accounting.Models
{
    public class DashboardVM
    {
        public List<MenusVM>? ParentMenus { get; set; }
        public List<SubMenusVM>? SubMenus { get; set; }
        public UserInfo? UserInfo { get; set; }
        public UserRoleVM? UserRole { get; set; }
    }

    public class UserRoleVM
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
    }
}
