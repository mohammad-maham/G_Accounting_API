using System;
using System.Collections.Generic;
using NodaTime;

namespace Accounting.Models;

public partial class ArcUser
{
    public long Id { get; set; }

    public decimal NationalCode { get; set; }

    public Instant RegDate { get; set; }

    public short Status { get; set; }

    public string? Email { get; set; }

    public decimal? Mobile { get; set; }

    public string? Otpinfo { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public Instant ArcDate { get; set; }
}
