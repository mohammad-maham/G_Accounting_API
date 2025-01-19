using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class RoleStateAccess
{
    public int Id { get; set; }

    public int RoleAccessId { get; set; }

    public int UserStateTypeId { get; set; }

    public int Status { get; set; }
}
