using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class SessionMgr
{
    public long Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTimeOffset UseDate { get; set; }

    public short Status { get; set; }
}
