using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class LabOrder
{
    public int OrderId { get; set; }

    public int MedicalRecordId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? Status { get; set; }

    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<LabOrderItem> LabOrderItems { get; set; } = new List<LabOrderItem>();

    public virtual MedicalRecord MedicalRecord { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
