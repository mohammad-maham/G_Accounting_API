﻿using System;
using System.Collections.Generic;
using NodaTime;

namespace Accounting.Models;

public partial class Contact
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public short Status { get; set; }

    public DateTime RegDate { get; set; }

    public int RegionId { get; set; }

    public List<long>? Tells { get; set; }

    public List<long>? Mobiles { get; set; }

    public string? Addresses { get; set; }
}
