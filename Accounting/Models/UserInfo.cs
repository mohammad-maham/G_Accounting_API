using System;
using System.Collections;
using System.Collections.Generic;
using NodaTime;

namespace Accounting.Models;

public partial class UserInfo
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public LocalDate? BirthDay { get; set; }

    public BitArray? Gender { get; set; }

    public string? FatherName { get; set; }

    public Instant RegDate { get; set; }

    public string? ShahkarInfo { get; set; }

    public short Status { get; set; }
}
