using System;
using System.Collections.Generic;
using NodaTime;

namespace Accounting.Models;

public partial class SessionMgr
{
    public long Id { get; set; }

    public string Token { get; set; } = null!;

    public short Status { get; set; }

    public DateTime UseDate { get; set; }
}
