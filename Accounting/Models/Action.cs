using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class Action
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public short Status { get; set; }

    public string Path { get; set; } = null!;

    public DateTime ArcDate { get; set; }
}