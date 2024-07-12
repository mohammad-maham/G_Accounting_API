namespace Accounting.Models;

public partial class UserRole
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public short Status { get; set; }

    public DateTime RegDate { get; set; }

    public long RegUserId { get; set; }

    public short RoleId { get; set; }
}
