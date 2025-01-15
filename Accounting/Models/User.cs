using System;
using System.Collections.Generic;
using NodaTime;

namespace Accounting.Models;

public partial class User
{
    public long Id { get; set; }

    public DateTime RegDate { get; set; }

    public short Status { get; set; }

    public string? Email { get; set; }

    public string? Otpinfo { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public long? Mobile { get; set; }

    public long NationalCode { get; set; }

    public string? IdentificationCode { get; set; }

    public string? ReferralCode { get; set; }

    public short UserType { get; set; }
}
