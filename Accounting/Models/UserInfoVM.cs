namespace Accounting.Models;

public class UserInfoVM
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? BirthDay { get; set; }

    public string? FatherName { get; set; }

    public DateTime RegDate { get; set; }

    public string? SedadInfo { get; set; }

    public short Status { get; set; }

    public short? Gender { get; set; }

    public string? NationalCardImage { get; set; }

    public int? UserRoleId { get; set; }

    public string? UserRole { get; set; }
}
public class UserInfoAuthVM
{
    public string? NationalCode { get; set; }
    public string? Name { get; set; }
    public string? Family { get; set; }
    public string? NationalId { get; set; }
    public string? Mobile { get; set; }
    public string? BirthDate { get; set; }
}