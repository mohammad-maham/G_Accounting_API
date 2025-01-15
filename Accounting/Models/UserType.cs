using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class UserType
{
    public short Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
