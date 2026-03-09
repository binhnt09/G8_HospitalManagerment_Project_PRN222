using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class PrescriptionItem
{
    public int PrescriptionItemId { get; set; }

    public int PrescriptionId { get; set; }

    public int DrugId { get; set; }

    public int? Quantity { get; set; }

    public string? Dosage { get; set; }

    public int? DurationDays { get; set; }

    public string? UsageInstructions { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Drug Drug { get; set; } = null!;

    public virtual Prescription Prescription { get; set; } = null!;
}
