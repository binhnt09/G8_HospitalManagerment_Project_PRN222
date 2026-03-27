using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class InpatientAdmission
{
    public int AdmissionId { get; set; }

    public int? PatientId { get; set; }

    public int? DoctorId { get; set; }

    public int? BedId { get; set; }

    public DateTime? AdmissionDate { get; set; }

    public DateTime? DischargeDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Bed? Bed { get; set; }

    public virtual ICollection<DailyCareRecord> DailyCareRecords { get; set; } = new List<DailyCareRecord>();

    public virtual Doctor? Doctor { get; set; }

    public virtual Patient? Patient { get; set; }
}
