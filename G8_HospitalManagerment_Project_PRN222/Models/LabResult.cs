using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class LabResult
{
    public int ResultId { get; set; }

    public int OrderItemId { get; set; }

    public string? ResultValue { get; set; }

    public bool? IsAbnormal { get; set; }

    public string? Remarks { get; set; }

    public int? PerformedBy { get; set; }

    public DateTime? ResultDate { get; set; }

    public virtual LabOrderItem OrderItem { get; set; } = null!;

    public virtual Employee? PerformedByNavigation { get; set; }
}
