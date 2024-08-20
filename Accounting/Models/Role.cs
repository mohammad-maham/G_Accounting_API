using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public short Status { get; set; }

    public string? Description { get; set; }
}
