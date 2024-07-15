using System.ComponentModel.DataAnnotations;

namespace Accounting.Models;

public partial class User
{
    public long Id { get; set; }

    public decimal NationalCode { get; set; }

    public DateTime RegDate { get; set; }

    public short Status { get; set; }

    public string? Email { get; set; }

    public decimal? Mobile { get; set; }

    public string? Otpinfo { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }
}

public class UserReq
{
    [Required, RegularExpression(@"[0-9]{10}", ErrorMessage = "Invalid national code!")]
    public decimal NationalCode { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required, RegularExpression(@"[0-9]{12}", ErrorMessage = "Invalid mobile, must be started (98)!")]
    public decimal? Mobile { get; set; }
}