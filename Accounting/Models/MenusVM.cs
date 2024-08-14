namespace Accounting.Models
{
    public class MenusVM
    {
        public int MenuId { get; set; }
        public string? MenuName { get; set; }
        public string? MenuTitle { get; set; }
        public string? MenuIcon { get; set; }
    }

    public class SubMenusVM
    {
        public int ActionId { get; set; }
        public string? ActionName { get; set; }
        public string? ActionPath { get; set; }
        public int? OwnerMenuId { get; set; }
    }
}
