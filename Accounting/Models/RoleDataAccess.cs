using System;
using System.Collections.Generic;

namespace Accounting.Models;

public partial class RoleDataAccess
{
    public int Id { get; set; }

    public int RoleAccessId { get; set; }

    public short DataAccessTypeId { get; set; }

    public int? RegionId { get; set; }

    public short Status { get; set; }
}
