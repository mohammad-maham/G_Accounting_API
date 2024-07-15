using System.Collections;

namespace Accounting.Models;

public partial class UserInfo
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? BirthDay { get; set; }

    public BitArray? Gender { get; set; }

    public string? FatherName { get; set; }

    public DateTime RegDate { get; set; }

    public string? SedadInfo { get; set; }

    public short Status { get; set; }
}
