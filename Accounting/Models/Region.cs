using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class Region
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public short Type { get; set; }

    public short Status { get; set; }

    public int? ParentId { get; set; }

    public int ProvinceCode { get; set; }
}
