using System;
using System.Collections.Generic;
using NodaTime;

namespace Accounting.Models;

public partial class LegalUserInfo
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public short Status { get; set; }

    public DateTime RegDate { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? RegistrationDate { get; set; }

    public decimal RegistrationNumber { get; set; }

    public int? RegistrationRegionId { get; set; }

    public DateTime? LastModifyInfoDate { get; set; }

    public int AgentRole { get; set; }
}
