namespace Accounting.Models;

public partial class UserSession
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public DateTime SessionDate { get; set; }

    public string SessionInfo { get; set; } = null!;
}
