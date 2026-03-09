using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class LabOrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int TestId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();

    public virtual LabOrder Order { get; set; } = null!;

    public virtual Test Test { get; set; } = null!;
}
