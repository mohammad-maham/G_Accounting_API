using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class User
{
    public long Id { get; set; }

    public decimal NationalCode { get; set; }

    public DateTime RegDate { get; set; }

    public short Status { get; set; }

    public string? Email { get; set; }

    public decimal Mobile { get; set; }

    public int? Otp { get; set; }
}
