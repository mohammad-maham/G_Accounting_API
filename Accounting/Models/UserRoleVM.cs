namespace Accounting.Models
{
    public class UsersRoleVM
    {
        public long? Id { get; set; }

        public long? UserId { get; set; }

        public short? Status { get; set; }

        public DateTime? RegDate { get; set; }

        public long? RegUserId { get; set; }

        public int? RoleId { get; set; }
    }
}
