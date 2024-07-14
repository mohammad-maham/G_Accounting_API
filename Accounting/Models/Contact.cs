using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class Contact
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public short Status { get; set; }

    public DateTime RegDate { get; set; }

    public int RegionId { get; set; }

    public List<string>? Addresses { get; set; }

    public List<decimal>? Tells { get; set; }

    public List<decimal>? Mobiles { get; set; }
}
