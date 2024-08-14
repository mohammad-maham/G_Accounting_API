using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class RoleAccess
{
    public int Id { get; set; }

    public short RoleId { get; set; }

    public int? MenuId { get; set; }

    public short Status { get; set; }

    public short ActionId { get; set; }
    public int? OwnerMenuId { get; set; }
}
