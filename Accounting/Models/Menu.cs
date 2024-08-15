using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class Menu
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public string Name { get; set; } = null!;

    public string? Title { get; set; }

    public short Status { get; set; }

    public string? Icon { get; set; }
}
