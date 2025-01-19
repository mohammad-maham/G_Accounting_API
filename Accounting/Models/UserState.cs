using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class UserState
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public int UserStateTypeId { get; set; }

    public int Status { get; set; }
}
